namespace WebSocketService
{
    public interface IJobFactory
    {
        bool AcceptProtocol(string protocol);

        string GetJobName(string protocol);

        int GetJobReceiveMessageBufferSize(string protocol);

        Job CreateJob(IWebSocketSession socketSession);
    }
}
