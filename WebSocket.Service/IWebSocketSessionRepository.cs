using System.Collections.Generic;

namespace WebSocketService
{
    public interface IWebSocketSessionRepository
    {
        IEnumerable<IWebSocketSession> GetActiveSessions();
    }
}
