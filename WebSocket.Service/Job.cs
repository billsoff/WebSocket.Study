using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public abstract class Job
    {
        protected Job()
        {
        }

        public int Id => SocketSession.Id;

        public string Name => SocketSession.Name;

        public IWebSocketSession SocketSession { get; internal set; }

        public IBroadcast Broadcast { get; set; }

        public bool IsSocketSessionActive => SocketSession.IsActive;

        internal protected CancellationToken ServerStoppingNotifier { get; set; }

        #region Job

        internal protected abstract Task ExecuteAsync();

        #endregion
    }
}
