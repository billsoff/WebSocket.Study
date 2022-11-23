﻿using System;
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

        public event EventHandler<JobEventArgs> JobCreated;
        public event EventHandler<JobEventArgs> JobTermited;
        public event EventHandler<JobFaultEventArgs> JobFault;
        public event EventHandler<JobEventArgs> JobRemoved;

        public string ListeningAddress { get; private set; }

        public WebSocketServerState State { get; private set; } = WebSocketServerState.Initial;

        public IList<Job> GetActiveJobs() => _jobRepository.GetActiveJobs();

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

            while (true)
            {
                if (socket.State != WebSocketState.Open)
                {
                    break;
                }

                Job job = _jobFactory.CreateJob(socket.SubProtocol);

                job.SocketContext = socketContext;

                _jobRepository.Register(job);

                JobCreated?.Invoke(this, new JobEventArgs(job, GetActiveJobs()));

                try
                {
                    await RunJobAsync(job);

                    if (!job.PermitSocketChannelReused && job.IsSocketChannelOpen)
                    {
                        await TerminateJobAsync(job);

                        break;
                    }
                }
                catch (WebSocketException ex)
                {
                    JobFault?.Invoke(this, new JobFaultEventArgs(job, GetActiveJobs(), ex));

                    break;
                }
                finally
                {
                    _jobRepository.Unregister(job);
                    JobRemoved?.Invoke(
                            this,
                            new JobEventArgs(job, GetActiveJobs())
                        );
                }
            }
        }

        private async Task RunJobAsync(Job job)
        {
            while (true)
            {
                await job.Run();

                if (!job.IsSocketChannelOpen || !job.IsReusable)
                {
                    if (!job.IsSocketChannelOpen)
                    {
                        await TerminateJobAsync(job);

                        JobTermited?.Invoke(this, new JobEventArgs(job, GetActiveJobs()));
                    }

                    break;
                }
            }
        }

        private async Task TerminateJobAsync(Job job)
        {
            WebSocket socket = job.Socket;

            switch (socket.State)
            {
                case WebSocketState.Open:
                    await socket.CloseAsync(
                           WebSocketCloseStatus.NormalClosure,
                           null,
                           CancellationToken.None
                       );

                    break;

                case WebSocketState.CloseReceived:
                    await socket.CloseOutputAsync(
                            WebSocketCloseStatus.NormalClosure,
                            null,
                            CancellationToken.None
                        );

                    break;

                case WebSocketState.None:
                case WebSocketState.Connecting:
                case WebSocketState.CloseSent:
                case WebSocketState.Closed:
                case WebSocketState.Aborted:
                default:
                    break;
            }
        }
    }
}
