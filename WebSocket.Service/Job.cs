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

        public int Id { get; private set; }

        public bool IsIdle => ExecutionStep != JobExecutionStep.Executing;

        public JobExecutionStep ExecutionStep { get; private set; } = JobExecutionStep.WaitNextReceive;

        public IWebSocketSession SocketSession { get; internal set; }

        public bool IsSocketSessionActive => SocketSession.IsActive;

        internal async Task Run()
        {
            ExecutionStep = JobExecutionStep.WaitNextReceive;

            await PreExecuteAsync();

            string message = await WaitJobCommand();

            if (message == null || !IsSocketSessionActive)
            {
                return;
            }

            ExecutionStep = JobExecutionStep.Executing;

            await ExecuteAsync(message);

            ExecutionStep = JobExecutionStep.Complete;
        }

        private async Task<string> WaitJobCommand()
        {
            if (IsExecutedAutomatically)
            {
                return null;
            }

            return await SocketSession.ReadMessageAsync();
        }

        #region Job

        protected virtual Task PreExecuteAsync()
        {
            return Task.FromResult<object>(null);
        }

        protected abstract Task ExecuteAsync(string message);

        public virtual bool IsReusable { get; }

        public virtual bool IsExecutedAutomatically { get; }

        public virtual bool PermitSocketChannelReused { get; }

        #endregion
    }
}
