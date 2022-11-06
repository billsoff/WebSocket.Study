using System.Threading.Tasks;

namespace WebSocketService
{
    public interface IJob
    {
        bool IsReusable { get; }

        bool Recognize(string message);

        Task Execute();
    }
}
