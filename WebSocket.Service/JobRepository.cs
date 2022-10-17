﻿using System.Collections.Generic;

namespace WebSocketService
{
    internal sealed class JobRepository
    {
        private readonly List<Job> _jobs = new List<Job>();
        private readonly object _locker = new object();

        public IList<Job> WorkingJobs
        {
            get
            {
                lock (_locker)
                {
                    return new List<Job>(_jobs);
                }
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
