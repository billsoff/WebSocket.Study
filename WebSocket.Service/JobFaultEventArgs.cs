using System.Collections.Generic;
using System.Net.WebSockets;

namespace WebSocketService
{
    public sealed class JobFaultEventArgs : JobEventArgs
    {
        public JobFaultEventArgs(Job job, IList<Job> activeJobs, WebSocketException exception)
            : base(job, activeJobs)
        {
            Exception = exception;
        }

        public WebSocketException Exception { get; private set; }
    }
}
