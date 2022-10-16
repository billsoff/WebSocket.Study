namespace WebSocketService
{
    public interface IJobInitializer<TJob>
        where TJob : Job, new()
    {
        void Initialize(TJob job);
    }
}
