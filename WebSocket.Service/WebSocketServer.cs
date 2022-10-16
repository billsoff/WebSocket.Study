using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketService
{
    public sealed class WebSocketServer<TJob>
        where TJob : Job, new()
    {
        private readonly HttpListener _listener;

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
            }
        }

        public void Stop()
        {
            _listener.Stop();
        }


        private async Task AcceptJob(WebSocketContext socketContext)
        {
            while (true)
            {
                TJob job = new TJob()
                {
                    SocketContext = socketContext
                };

                JobPolicyOnCompletion policy = await job.Run();

                if (policy == JobPolicyOnCompletion.Termiante)
                {
                    break;
                }
            }
        }
    }
}
