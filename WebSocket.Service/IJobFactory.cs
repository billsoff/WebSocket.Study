namespace WebSocketService
{
    public interface IJobFactory
    {
        bool AcceptProtocol(string protocol);

        int GetJobReceiveMessageBufferSize(string protocol);

        Job CreateJob(IWebSocketSession socketSession);
    }
}
