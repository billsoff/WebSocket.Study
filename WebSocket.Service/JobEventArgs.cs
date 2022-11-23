using System;
using System.Collections.Generic;

namespace WebSocketService
{
    public class JobEventArgs : EventArgs
    {
        public JobEventArgs(Job job, IList<Job> activeJobs)
        {
            Job = job;
            ActiveJobs = activeJobs;
        }

        public Job Job { get; private set; }

        public IList<Job> ActiveJobs { get; private set; }
    }
}
