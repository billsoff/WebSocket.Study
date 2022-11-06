using System.Collections.Generic;
using System.Net.WebSockets;

namespace WebSocketService
{
    public sealed class JobFaultEventArgs<TJob> : JobEventArgs<TJob>
        where TJob : Job
    {
        public JobFaultEventArgs(TJob job, IList<TJob> activeJobs, WebSocketException exception)
            : base(job, activeJobs)
        {
            Exception = exception;
        }

        public WebSocketException Exception { get; private set; }
    }
}
