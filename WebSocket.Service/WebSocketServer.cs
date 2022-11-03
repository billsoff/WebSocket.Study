using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
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
            _listener.Start();

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
                tasks.Add(job.Terminate());
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
                if (socketContext.WebSocket.State != WebSocketState.Open)
                {
                    return;
                }

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
                        await job.Terminate();

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                finally
                {
                    _jobRepository.Unregister(job);
                    Console.WriteLine("Socket status: {0}", socketContext.WebSocket.State);
                    Console.WriteLine("Job count: {0}", _jobRepository.WorkingJobs.Count);
                }
            }
        }
    }
}
