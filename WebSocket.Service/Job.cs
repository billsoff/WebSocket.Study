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
        private JobExecutionStep _executionStep;

        internal WebSocketContext SocketContext { get; set; }

        private WebSocket Socket { get => SocketContext.WebSocket; }

        internal async Task<JobPolicyOnCompletion> Run()
        {
            while (true)
            {
                string message = await ReceiveAsync();
                bool recognized = Recognize(message);

                if (!recognized)
                {
                    return JobPolicyOnCompletion.ContinueNextJob;
                }

                _executionStep = await Execute();

                if (_executionStep == JobExecutionStep.Complete)
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

        internal async Task Terminate()
        {
            if (_executionStep != JobExecutionStep.Complete)
            {
                const int WAIT_MAX_SECONDS = 20;

                for (int i = 0; i < WAIT_MAX_SECONDS; i++)
                {
                    await Task.Delay(1000); // 1s

                    if (_executionStep == JobExecutionStep.Complete)
                    {
                        break;
                    }
                }
            }

            await Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    null,
                    CancellationToken.None
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

        private static bool LogException(Exception　e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.GetBaseException());
            return false;
        }
    }
}
