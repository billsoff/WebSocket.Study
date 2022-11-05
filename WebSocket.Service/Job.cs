using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public abstract class Job : IJob
    {
        private readonly ArraySegment<byte> _buffer = new ArraySegment<byte>(new byte[1024 * 1024]); // 1M

        internal WebSocketContext SocketContext { get; set; }

        internal WebSocket Socket { get => SocketContext.WebSocket; }

        public JobExecutionStep ExecutionStep { get; private set; }

        internal async Task<JobPolicyOnCompletion> Run()
        {
            while (true)
            {
                string message = await ReceiveAsync();

                if (Socket.State != WebSocketState.Open)
                {
                    return JobPolicyOnCompletion.Termiante;
                }

                bool recognized = Recognize(message);

                if (!recognized)
                {
                    return JobPolicyOnCompletion.ContinueNextJob;
                }

                ExecutionStep = await Execute();

                if (ExecutionStep == JobExecutionStep.Complete)
                {
                    break;
                }
            }


            return DeterminePolicyOnCompletion();
        }

        #region Job

        public abstract bool Recognize(string message);

        public abstract Task<JobExecutionStep> Execute();

        public abstract JobPolicyOnCompletion DeterminePolicyOnCompletion();

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

        private async Task<string> ReceiveAsync()
        {
            try
            {
                WebSocketReceiveResult result = await Socket.ReceiveAsync(_buffer, CancellationToken.None);
                string message = Encoding.UTF8.GetString(
                        _buffer.Array,
                        0,
                        result.Count
                    );

                return message;
            }
            catch (Exception ex) when (LogException(ex))
            {
                // 何も処理しない
                throw;
            }
        }

        private static bool LogException(Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.GetBaseException());
            return false;
        }
    }
}
