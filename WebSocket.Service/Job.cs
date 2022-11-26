using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public abstract class Job
    {
        private static int _sequence;

        protected Job()
        {
            Id = Interlocked.Increment(ref _sequence);
        }

        public int Id { get; private set; }

        public IWebSocketSession SocketSession { get; internal set; }

        public bool IsSocketSessionActive => SocketSession.IsActive;

        #region Job

        internal protected abstract Task ExecuteAsync();

        #endregion
    }
}
