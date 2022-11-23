namespace WebSocketService
{
    public interface IJobFactory
    {
        bool AcceptProtocol(string protocol);

        Job CreateJob(string protocol);
    }
}
