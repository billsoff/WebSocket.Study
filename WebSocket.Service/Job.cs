using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public abstract class Job
    {
        private readonly ArraySegment<byte> _buffer = new ArraySegment<byte>(new byte[1024 * 1024]); // 1M

        internal WebSocketContext SocketContext { get; set; }

        internal WebSocket Socket { get => SocketContext.WebSocket; }

        public bool IsActive { get => Socket.State == WebSocketState.Open; }

        public bool IsIdle { get => ExecutionStep == JobExecutionStep.WaitNextReceive; }

        public JobExecutionStep ExecutionStep { get; private set; } = JobExecutionStep.WaitNextReceive;

        public WebSocketState WebSocketState { get => Socket.State; }

        public WebSocketCloseStatus? WebSocketCloseStatus { get => Socket.CloseStatus; }

        internal async Task Run()
        {
            ExecutionStep = JobExecutionStep.WaitNextReceive;

            string message = await ReceiveAsync();

            if (Socket.State != WebSocketState.Open)
            {
                return;
            }

            bool recognized = Recognize(message);

            if (!recognized)
            {
                return;
            }

            ExecutionStep = JobExecutionStep.Executing;

            await Execute();

            ExecutionStep = JobExecutionStep.Complete;
        }

        #region Job

        public abstract bool Recognize(string message);

        public abstract Task Execute();

        public abstract bool IsReusable { get; }

        #endregion

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

        protected async Task<string> ReceiveAsync()
        {
            WebSocketReceiveResult result = await Socket.ReceiveAsync(_buffer, CancellationToken.None);
            string message = Encoding.UTF8.GetString(
                    _buffer.Array,
                    0,
                    result.Count
                );

            return message;
        }

        protected async Task CloseAsync()
        {
            await Socket.CloseAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    null,
                    CancellationToken.None
                );
        }
    }
}
