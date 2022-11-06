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

        private readonly JobRepository<TJob> _jobRepository = new JobRepository<TJob>();
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

            ListeningAddress = listeningAddress;
        }

        public event EventHandler Ready;
        public event EventHandler<WebSocketServerFaultEventArgs> Fault;
        public event EventHandler Stopped;

        public event EventHandler<JobEventArgs<TJob>> JobCreated;
        public event EventHandler<JobEventArgs<TJob>> JobTermited;
        public event EventHandler<JobFaultEventArgs<TJob>> JobFault;
        public event EventHandler<JobEventArgs<TJob>> JobRemoved;

        public string ListeningAddress { get; private set; }

        public WebSocketServerState State { get; private set; } = WebSocketServerState.Initial;

        public IList<TJob> GetActiveJobs() => _jobRepository.GetActiveJobs();

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                State = WebSocketServerState.Fault;
                Fault?.Invoke(this, new WebSocketServerFaultEventArgs(ex));

                return;
            }

            State = WebSocketServerState.Running;
            Ready?.Invoke(this, EventArgs.Empty);

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

            foreach (Job job in GetActiveJobs())
            {
                tasks.Add(Terminate(job));
            }

            Task.WaitAll(tasks.ToArray());

            _listener.Stop();

            State = WebSocketServerState.Stopped;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Stop();
        }

        private async Task AcceptJob(WebSocketContext socketContext)
        {
            WebSocket socket = socketContext.WebSocket;

            while (true)
            {
                if (socket.State != WebSocketState.Open)
                {
                    break;
                }

                TJob job = new TJob
                {
                    SocketContext = socketContext,
                };

                _jobInitializer?.Initialize(job);
                _jobRepository.Register(job);

                JobCreated?.Invoke(this, new JobEventArgs<TJob>(job, GetActiveJobs()));

                try
                {
                    await RunJob(job);
                }
                catch (WebSocketException ex)
                {
                    JobFault?.Invoke(this, new JobFaultEventArgs<TJob>(job, GetActiveJobs(), ex));

                    break;
                }
                finally
                {
                    _jobRepository.Unregister(job);
                    JobRemoved?.Invoke(this, new JobEventArgs<TJob>(job, GetActiveJobs()));
                }
            }
        }

        private async Task RunJob(TJob job)
        {
            while (true)
            {
                await job.Run();

                if (!job.IsActive || !job.IsReusable)
                {
                    if (!job.IsActive)
                    {
                        await Terminate(job);

                        JobTermited?.Invoke(this, new JobEventArgs<TJob>(job, GetActiveJobs()));
                    }

                    break;
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
            if (!job.IsActive || job.IsIdle)
            {
                return;
            }

            const int WAIT_MAX_SECONDS = 20;

            for (int i = 0; i < WAIT_MAX_SECONDS; i++)
            {
                await Task.Delay(1000); // 1s

                if (!job.IsActive || job.IsIdle)
                {
                    return;
                }
            }
        }
    }
}
