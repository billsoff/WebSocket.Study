using System;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    internal sealed class EchoJob : Job
    {
        internal string Suffix { get; set; }

        protected override async Task ExecuteAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Console.WriteLine("{0} (from job {1} count: {2:#,##0})", message, Id, message.Length);
            await SendAsync($"Hello! {message} {Suffix}");
        }

        public override bool IsReusable => true;
    }
}
