using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocket socket = CreateSocket().Result;

            socket.SendMessageAsync("Sweet").Wait();

            string echo = socket.ReadMessageAsync().Result;

            Console.WriteLine(echo);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static async Task<ClientWebSocket> CreateSocket()
        {
            ClientWebSocket socket = new ClientWebSocket();

            await socket.ConnectAsync(new Uri("ws://127.0.0.1:8089/"), CancellationToken.None);

            return socket;
        }
    }
}
