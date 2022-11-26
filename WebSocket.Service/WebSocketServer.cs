using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public sealed class WebSocketServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly IJobFactory _jobFactory;

        private readonly JobRepository _jobRepository = new JobRepository();

        public WebSocketServer(string listeningAddress, IJobFactory jobFactory)
        {
            _jobFactory = jobFactory;

            _listener = new HttpListener();
            _listener.Prefixes.Add(listeningAddress);

            ListeningAddress = listeningAddress;
        }

        public event EventHandler Ready;
        public event EventHandler<WebSocketServerFaultEventArgs> Fault;
        public event EventHandler Stopping;
        public event EventHandler Stopped;

        public event EventHandler<JobEventArgs> JobStart;
        public event EventHandler<JobCompleteEventArgs> JobComplete;

        public string ListeningAddress { get; private set; }

        public WebSocketServerState State { get; private set; } = WebSocketServerState.Initial;

        public IList<Job> GetActiveJobs() => _jobRepository.GetActiveJobs();

        public async Task StartAsync()
        {
            if (State != WebSocketServerState.Initial)
            {
                throw new InvalidOperationException($"Now the state is {State}, so cannot be started again.");
            }

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

                if (!listenerContext.Request.IsWebSocketRequest)
                {
                    RejectInvalidHttpRequest(listenerContext);

                    continue;
                }

                WebSocketContext socketContext = await AcceptWebSocketAsync(listenerContext);

                if (socketContext != null)
                {
                    Task _ = AcceptJobAsync(socketContext);
                }
            }
        }

        private void RejectInvalidHttpRequest(HttpListenerContext listenerContext)
        {
            listenerContext.Response.StatusCode = 404;
            listenerContext.Response.Close();
        }

        private async Task<WebSocketContext> AcceptWebSocketAsync(HttpListenerContext listenerContext)
        {
            IList<string> protocols = GetProtocols(listenerContext);

            if (GetAcceptedProtocol(protocols, out string protocol))
            {
                return await listenerContext.AcceptWebSocketAsync(protocol);
            }

            WebSocketContext socketContext = await listenerContext.AcceptWebSocketAsync(protocols.FirstOrDefault());
            await socketContext.WebSocket.CloseAsync(
                    WebSocketCloseStatus.ProtocolError,
                    $"None of \"{string.Join(",", socketContext.SecWebSocketProtocols)}\" protocols is supported.",
                    CancellationToken.None
                );

            return null;
        }

        private bool GetAcceptedProtocol(IList<string> protocols, out string protocol)
        {
            if (protocols.Count == 0)
            {
                protocol = null;

                return _jobFactory.AcceptProtocol(null);
            }

            protocol = protocols.FirstOrDefault(p => _jobFactory.AcceptProtocol(p));

            return protocol != null;
        }

        private IList<string> GetProtocols(HttpListenerContext listenerContext)
        {
            string values = listenerContext.Request.Headers["Sec-WebSocket-Protocol"]?.Trim();

            if (string.IsNullOrEmpty(values))
            {
                return new string[0];
            }

            return Regex.Split(values, @"\s*,\s*");
        }

        public void Stop()
        {
            if (State != WebSocketServerState.Running)
            {
                return;
            }

            State = WebSocketServerState.Stopping;

            Stopping?.Invoke(this, EventArgs.Empty);

            _listener.Stop();

            State = WebSocketServerState.Stopped;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Stop();
        }

        private async Task AcceptJobAsync(WebSocketContext socketContext)
        {
            WebSocket socket = socketContext.WebSocket;
            WebSocketSession webSocketSession = new WebSocketSession(
                    socket,
                    _jobFactory.GetJobReceiveMessageBufferSize(socket.SubProtocol)
                );

            if (!webSocketSession.IsActive)
            {
                return;
            }

            Job job = _jobFactory.CreateJob(webSocketSession);

            job.SocketSession = webSocketSession;

            _jobRepository.Register(job);

            JobStart?.Invoke(this, new JobEventArgs(job, GetActiveJobs()));

            try
            {
                await job.ExecuteAsync();

                JobComplete?.Invoke(this, new JobCompleteEventArgs(job, GetActiveJobs()));
            }
            catch (Exception ex)
            {
                JobComplete?.Invoke(this, new JobCompleteEventArgs(job, GetActiveJobs(), ex));
            }
            finally
            {
                _jobRepository.Unregister(job);
                await webSocketSession.CloseAsync();
            }
        }
    }
}
