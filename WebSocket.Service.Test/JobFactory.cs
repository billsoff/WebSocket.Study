namespace WebSocketService.Test
{
    internal sealed class JobFactory : IJobFactory
    {
        private readonly string _suffix;

        public JobFactory(string suffix)
        {
            _suffix = suffix;
        }

        public bool AcceptProtocol(string protocol)
        {
            switch (protocol)
            {
                case null:
                    return true;

                default:
                    return false;
            }
        }

        public int GetJobReceiveMessageBufferSize(string protocol)
        {
            return 1024;
        }

        public Job CreateJob(IWebSocketSession socketSession)
        {
            switch (socketSession.Protocol)
            {
                case null:
                default:
                    return new EchoJob
                    {
                        Suffix = _suffix,
                    };
            }
        }
    }
}
