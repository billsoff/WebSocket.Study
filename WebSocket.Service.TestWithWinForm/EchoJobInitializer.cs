namespace WebSocketService.TestWithWinForm
{
    internal sealed class EchoJobInitializer : IJobInitializer<EchoJob>
    {
        private readonly string _suffix;
        private readonly INotifier _notifer;

        public EchoJobInitializer(string suffix, INotifier notifier)
        {
            _suffix = suffix;
            _notifer = notifier;
        }

        public void Initialize(EchoJob job)
        {
            job.Suffix = _suffix;
            job.Notifier = _notifer;
        }
    }
}
