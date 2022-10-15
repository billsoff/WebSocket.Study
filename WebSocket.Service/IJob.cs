using System.Threading.Tasks;

namespace WebSocketService
{
    public interface IJob
    {
        bool Recognize(string message);

        Task<JobExecutionStep> Execute();

        bool ContinueNextJob();
    }
}
