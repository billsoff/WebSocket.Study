using System;
using System.Threading.Tasks;

namespace WebSocketService.TestWithWinForm
{
    internal sealed class EchoJob : Job
    {
        internal string Suffix { get; set; }

        internal INotifier Notifier { get; set; }

        protected override async Task ExecuteAsync()
        {
            string message;

            while (SocketSession.IsActive)
            {
                message = await SocketSession.ReceiveMessageAsync();

                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

                Console.WriteLine(message);
                Notifier.Notify(message);

                await SocketSession.SendMessageAsync($"Hello! {message} {Suffix}");
            }
        }
    }
}
