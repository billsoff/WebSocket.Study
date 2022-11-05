using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public sealed class WebSocketServer<TJob> : IDisposable
        where TJob : Job, new()
    {
        private readonly HttpListener _listener;

        private readonly JobRepository _jobRepository = new JobRepository();
        private volatile bool _terminate;

        private readonly IJobInitializer<TJob> _jobInitializer;

        public WebSocketServer(string listeningAddress)
            : this(listeningAddress, null)
        {
        }

        public WebSocketServer(string listeningAddress, IJobInitializer<TJob> jobInitializer)
        {
            _jobInitializer = jobInitializer;

            _listener = new HttpListener();
            _listener.Prefixes.Add(listeningAddress);
        }

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                // TODO: StatusEvent
                Console.WriteLine(ex);

                return;
            }

            while (true)
            {
                HttpListenerContext listenerContext = await _listener.GetContextAsync();
                WebSocketContext socketContext = await listenerContext.AcceptWebSocketAsync(null);

                Task _ = AcceptJob(socketContext);

                if (_terminate)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            _terminate = true;

            List<Task> tasks = new List<Task>();

            foreach (Job job in _jobRepository.WorkingJobs)
            {
                tasks.Add(Terminate(job));
            }

            Task.WaitAll(tasks.ToArray());

            _listener.Stop();
        }

        public void Dispose()
        {
            Stop();
        }

        private async Task AcceptJob(WebSocketContext socketContext)
        {
            while (true)
            {
                TJob job = new TJob
                {
                    SocketContext = socketContext,
                };

                _jobInitializer?.Initialize(job);
                _jobRepository.Register(job);

                Console.WriteLine("Job count: {0}", _jobRepository.WorkingJobs.Count);

                try
                {
                    JobPolicyOnCompletion policy = await job.Run();

                    if (policy == JobPolicyOnCompletion.Termiante)
                    {
                        await Terminate(job);

                        return;
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    _jobRepository.Unregister(job);
                    Console.WriteLine("Socket state: {0}", socketContext.WebSocket.State);
                    Console.WriteLine("Socket close status: {0}", socketContext.WebSocket.CloseStatus);
                    Console.WriteLine("Job count: {0}", _jobRepository.WorkingJobs.Count);
                }
            }
        }

        private async Task Terminate(Job job)
        {
            WebSocket socket = job.Socket;

            switch (socket.State)
            {
                case WebSocketState.Open:
                    await WaitJobComplete(job);

                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.CloseAsync(
                               WebSocketCloseStatus.NormalClosure,
                               null,
                               CancellationToken.None
                           );
                    }
                    else
                    {
                        await Terminate(job);
                    }

                    return;

                case WebSocketState.CloseReceived:
                    await socket.CloseOutputAsync(
                            WebSocketCloseStatus.NormalClosure,
                            null,
                            CancellationToken.None
                        );

                    return;

                case WebSocketState.None:
                case WebSocketState.Connecting:
                case WebSocketState.CloseSent:
                case WebSocketState.Closed:
                case WebSocketState.Aborted:
                default:
                    return;
            }
        }

        private async Task WaitJobComplete(Job job)
        {
            if (job.ExecutionStep == JobExecutionStep.Complete)
            {
                return;
            }

            const int WAIT_MAX_SECONDS = 20;

            for (int i = 0; i < WAIT_MAX_SECONDS; i++)
            {
                await Task.Delay(1000); // 1s

                if (job.Socket.State != WebSocketState.Open)
                {
                    return;
                }

                if (job.ExecutionStep == JobExecutionStep.Complete)
                {
                    return;
                }
            }
        }
    }
}
