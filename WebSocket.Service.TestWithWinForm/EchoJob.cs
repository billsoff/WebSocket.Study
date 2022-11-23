using System;
using System.Threading.Tasks;

namespace WebSocketService.TestWithWinForm
{
    internal sealed class EchoJob : Job
    {
        private string _message;

        internal string Suffix { get; set; }

        internal INotifier Notifier { get; set; }

        protected override async Task ExecuteAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Console.WriteLine(message);
            Notifier.Notify(message);

            await SendAsync($"Hello! {_message} {Suffix}");
        }
    }
}
