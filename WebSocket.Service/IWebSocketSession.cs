using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketService
{
    public interface IWebSocketSession : IDisposable
    {
        bool IsSessionActive { get; }

        bool HasMessages();

        Task<string> ReadMessageAsync(TimeSpan timeout = default(TimeSpan));

        Task<bool> SendMessageAsync(string message);
    }
}
