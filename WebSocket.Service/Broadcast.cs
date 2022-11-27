using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketService
{
    internal sealed class Broadcast : IBroadcast
    {
        private readonly IWebSocketSessionRepository _socketSessionRepository;
        private readonly IWebSocketSession _current;

        public Broadcast(IWebSocketSessionRepository socketSessionRepository, IWebSocketSession current)
        {
            _socketSessionRepository = socketSessionRepository;
            _current = current;
        }

        public async Task BroadcastMessageAsync(string message, bool excludeSelf = false)
        {
            IEnumerable<IWebSocketSession> allSessions = _socketSessionRepository.GetActiveSessions();

            if (excludeSelf)
            {
                allSessions = allSessions.Where(session => session != _current);
            }

            await Task.WhenAll(allSessions.Select(session => session.SendMessageAsync(message)));
        }
    }
}
