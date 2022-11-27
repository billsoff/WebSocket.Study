using System.Collections.Generic;

namespace WebSocketService
{
    public interface IWebSocketSessionRepository
    {
        bool HasActiveSessions { get; }

        IEnumerable<IWebSocketSession> GetActiveSessions();
    }
}
