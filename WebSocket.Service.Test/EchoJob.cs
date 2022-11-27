using System;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    internal sealed class EchoJob : Job
    {
        internal string Suffix { get; set; }

        protected override async Task ExecuteAsync()
        {
            string message;

            while (SocketSession.IsActive)
            {
                message = await SocketSession.ReceiveMessageAsync();

                if (string.IsNullOrWhiteSpace(message))
                {
                    break;
                }

                Console.WriteLine("{0} (from job {1} count: {2:#,##0})", message, Id, message.Length);

                await SocketSession.SendMessageAsync($"Hello! {message} {Suffix}");
                await SocketSession.BroadcastMessageAsync(message, excludeSelf: true);
            }
        }
    }
}
