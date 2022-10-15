using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public abstract class Job : IJob
    {
        private readonly ArraySegment<byte> _buffer = new ArraySegment<byte>(new byte[1024 * 1024]); // 1M

        internal async Task<bool> Run()
        {
            while (true)
            {
                string message = await ReceiveAsync();
                bool recognized = Recognize(message);

                if (!recognized)
                {
                    return true;
                }

                JobExecutionStep step = await Execute();

                if (step == JobExecutionStep.Complete)
                {
                    break;
                }
            }

            return ContinueNextJob();
        }

        public abstract bool Recognize(string message);

        public abstract Task<JobExecutionStep> Execute();

        public abstract bool ContinueNextJob();

        protected async Task SendAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            await Socket.SendAsync(
                    new ArraySegment<byte>(data),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None
                );
        }

        internal WebSocketContext SocketContext { get; set; }

        private WebSocket Socket { get => SocketContext.WebSocket; }

        private async Task<string> ReceiveAsync()
        {
            WebSocketReceiveResult result = await Socket.ReceiveAsync(_buffer, CancellationToken.None);
            string message = Encoding.UTF8.GetString(
                    _buffer.Array,
                    0,
                    result.Count
                );

            return message;
        }
    }
}
