using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WebSocketService;

namespace WebSocketClient.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebSocketSession session = CreateSocket().Result;

            session.SendMessageAsync("Sweet").Wait();

            string echo = session.ReceiveMessageAsync().Result;

            Console.WriteLine(echo);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static async Task<IWebSocketSession> CreateSocket()
        {
            ClientWebSocket socket = new ClientWebSocket();

            await socket.ConnectAsync(new Uri("ws://127.0.0.1:8089/"), CancellationToken.None);

            return new WebSocketSession(socket);
        }
    }
}
