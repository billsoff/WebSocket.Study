namespace WebSocketService.Test
{
    internal sealed class EchoJobInitializer : IJobInitializer<EchoJob>
    {
        private readonly string _suffix;

        public EchoJobInitializer(string suffix)
        {
            _suffix = suffix;
        }

        public void Initialize(EchoJob job)
        {
            job.Suffix = _suffix;
        }
    }
}
