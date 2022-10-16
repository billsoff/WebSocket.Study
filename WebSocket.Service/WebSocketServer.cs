using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketService
{
    public sealed class WebSocketServer<TJob>
        where TJob : Job, new()
    {
        private readonly HttpListener _listener;

        private readonly JobRepository _jobRepository = new JobRepository();
        private volatile bool _terminate;

        public WebSocketServer(string listeningAddress)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(listeningAddress);
        }

        public async Task Start()
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

        private async Task AcceptJob(WebSocketContext socketContext)
        {
            while (true)
            {
                if (socketContext.WebSocket.State != WebSocketState.Open)
                {
                    return;
                }

                TJob job = new TJob()
                {
                    SocketContext = socketContext,
                };

                _jobRepository.Register(job);

                try
                {
                    JobPolicyOnCompletion policy = await job.Run();

                    if (policy == JobPolicyOnCompletion.Termiante)
                    {
                        await job.Terminate();

                        break;
                    }
                }
                finally
                {
                    _jobRepository.Unregister(job);
                }
            }
        }
    }
}
