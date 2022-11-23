using System.Collections.Generic;
using System.Linq;

namespace WebSocketService
{
    internal sealed class JobRepository
    {
        private readonly List<Job> _jobs = new List<Job>();
        private readonly object _locker = new object();

        public IList<Job> GetActiveJobs()
        {
            lock (_locker)
            {
                return new List<Job>(_jobs.Where(job => job.IsSocketChannelOpen));
            }
        }

        public void Register(Job job)
        {
            lock (_locker)
            {
                _jobs.Add(job);
            }
        }

        public void Unregister(Job job)
        {
            lock (_locker)
            {
                _jobs.Remove(job);
            }
        }
    }
}
