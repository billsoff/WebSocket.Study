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

        public IWebSocketSession SocketSession { get; internal set; }

        public bool IsSocketSessionActive => SocketSession.IsActive;

        #region Job

        internal protected abstract Task ExecuteAsync();

        #endregion
    }
}
