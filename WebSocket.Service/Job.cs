using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public abstract class Job
    {
        private static int _sequence;

        private const int BUFFER_SIZE = 1024 * 10; // 10K

        private readonly ArraySegment<byte> _dataBuffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
        private readonly char[] _charBuffer = new char[Encoding.UTF8.GetMaxByteCount(BUFFER_SIZE)];

        protected Job()
        {
            Id = Interlocked.Increment(ref _sequence);
        }

        internal WebSocketContext SocketContext { get; set; }

        internal WebSocket Socket { get => SocketContext.WebSocket; }

        public int Id { get; private set; }

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
            Decoder decoder = Encoding.UTF8.GetDecoder();
            StringBuilder buffer = new StringBuilder();
            WebSocketReceiveResult result;

            do
            {
                result = await Socket.ReceiveAsync(_dataBuffer, CancellationToken.None);

                DecodeMessage(
                        decoder,
                        _dataBuffer.Array,
                        result.Count,
                        result.EndOfMessage,
                        buffer
                    );
            } while (!result.EndOfMessage);

            return buffer.ToString();
        }

        protected async Task CloseAsync()
        {
            await Socket.CloseAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    null,
                    CancellationToken.None
                );
        }

        private void DecodeMessage(Decoder decoder, byte[] data, int byteCount, bool hasNoMoreData, StringBuilder buffer)
        {
            int index = 0;
            int bytesRead = byteCount;
            int bytesUsed = 0;
            int charsUsed;
            bool completed;

            do
            {
                decoder.Convert(
                    data,
                    index,
                    bytesRead,
                    _charBuffer,
                    0,
                    _charBuffer.Length,
                    hasNoMoreData,
                    out bytesUsed,
                    out charsUsed,
                    out completed
                );

                buffer.Append(_charBuffer, 0, charsUsed);

                index += bytesUsed;
                bytesRead -= bytesUsed;

                if (bytesRead == 0)
                {
                    break;
                }
            } while (!completed);
        }
    }
}
