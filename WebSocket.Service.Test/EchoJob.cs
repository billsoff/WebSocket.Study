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

            while (SocketSession.IsActive && !ServerStoppingNotifier.IsCancellationRequested)
            {
                message = await SocketSession.ReceiveMessageAsync(TimeSpan.FromSeconds(1));

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                Console.WriteLine("{0} (from job {1} count: {2:#,##0})", message, Id, message.Length);

                await SocketSession.SendMessageAsync($"Hello! {message} {Suffix}");
                await Broadcast.BroadcastMessageAsync(message, excludeSelf: true);
            }
        }
    }
}
