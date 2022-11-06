using System;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    internal sealed class EchoJob : Job
    {
        private string _message;

        internal string Suffix { get; set; }

        public override bool Recognize(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            Console.WriteLine("{0} (from job {1})", message, Id);
            _message = message;

            return true;
        }

        public override async Task Execute()
        {
            await SendAsync($"Hello! {_message} {Suffix}");
        }

        public override bool IsReusable
        {
            get => true;
        }
    }
}
