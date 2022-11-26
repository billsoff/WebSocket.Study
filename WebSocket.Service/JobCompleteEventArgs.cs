using System;
using System.Collections.Generic;

namespace WebSocketService
{
    public sealed class JobCompleteEventArgs : JobEventArgs
    {
        public JobCompleteEventArgs(Job job, IList<Job> activeJobs, Exception exception = null)
            : base(job, activeJobs)
        {
            Exception = exception;
        }

        public bool Success => Exception == null;

        public Exception Exception { get; private set; }
    }
}
