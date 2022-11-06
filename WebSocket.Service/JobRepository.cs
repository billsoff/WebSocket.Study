using System.Collections.Generic;
using System.Linq;

namespace WebSocketService
{
    internal sealed class JobRepository<TJob>
        where TJob : Job
    {
        private readonly List<TJob> _jobs = new List<TJob>();
        private readonly object _locker = new object();

        public IList<TJob> GetActiveJobs()
        {
            lock (_locker)
            {
                return new List<TJob>(_jobs.Where(job => job.IsActive));
            }
        }

        public void Register(TJob job)
        {
            lock (_locker)
            {
                _jobs.Add(job);
            }
        }

        public void Unregister(TJob job)
        {
            lock (_locker)
            {
                _jobs.Remove(job);
            }
        }
    }
}
