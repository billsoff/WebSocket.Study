using System.Threading.Tasks;

namespace WebSocketService
{
    public interface IBroadcast
    {
        Task BroadcastMessageAsync(string message, bool excludeSelf = false);
    }
}
