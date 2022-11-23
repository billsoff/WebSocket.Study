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

        public Job CreateJob(string protocol)
        {
            switch (protocol)
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
