using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer<EchoJob> server = new WebSocketServer<EchoJob>("http://127.0.0.1:8089/");

            Task _ = server.Start();

            Console.WriteLine("Start web socket at http://127.0.0.1:8089/");

            Console.WriteLine("Enter exit to stop the server");

            while (true)
            {
                string line = Console.ReadLine();

                if (string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    server.Stop();

                    break;
                }

                Thread.Sleep(2000);
            }
        }
    }
}
