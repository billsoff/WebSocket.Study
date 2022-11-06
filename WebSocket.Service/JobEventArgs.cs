using System;
using System.Collections.Generic;

namespace WebSocketService
{
    public class JobEventArgs<TJob> : EventArgs
        where TJob : Job
    {
        public JobEventArgs(TJob job, IList<TJob> activeJobs)
        {
            Job = job;
            ActiveJobs = activeJobs;
        }

        public TJob Job { get; private set; }

        public IList<TJob> ActiveJobs { get; private set; }
    }
}
