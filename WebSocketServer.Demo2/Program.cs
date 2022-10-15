using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketServer.Demo2
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();

            listener.Prefixes.Add("http://127.0.0.1:8089/");
            listener.Start();

            Task _ = AcceptSocket(listener);

            Console.WriteLine("Start HTTP listener...");
            Console.ReadKey(true);

            listener.Stop();
        }

        private static async Task AcceptSocket(HttpListener listener)
        {
            HttpListenerContext listenerContext = await listener.GetContextAsync();
            WebSocketContext socketContext = await listenerContext.AcceptWebSocketAsync(null);

            while (true)
            {
                WebSocket socket = socketContext.WebSocket;
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1000]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                Console.WriteLine(message);

                string response = $"GET: {message}";
                byte[] encoded = Encoding.UTF8.GetBytes(response);

                await socket.SendAsync(new ArraySegment<byte>(encoded), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
