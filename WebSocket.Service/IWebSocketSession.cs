using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketService
{
    public interface IWebSocketSession : IDisposable
    {
        int Id { get; }

        string Name { get; }

        string Protocol { get; }

        bool IsActive { get; }

        WebSocketState State { get; }

        WebSocketCloseStatus? CloseStatus { get; }

        string CloseStatusDescription { get; }

        bool HasMessages();

        Task<string> ReceiveMessageAsync(TimeSpan timeout = default(TimeSpan));

        Task<bool> SendMessageAsync(string message);

        Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string reason = null);
    }
}
