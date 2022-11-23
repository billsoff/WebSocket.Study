namespace WebSocketService.TestWithWinForm
{
    internal sealed class JobFactory : IJobFactory
    {
        private readonly string _suffix;
        private readonly INotifier _notifer;

        public JobFactory(string suffix, INotifier notifier)
        {
            _suffix = suffix;
            _notifer = notifier;
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
                        Notifier = _notifer,
                    };
            }
        }
    }
}
