using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Console.OutputEncoding = Encoding.UTF8;

            string address = "http://127.0.0.1:8089/";
            EchoJobInitializer jobInitializer = new EchoJobInitializer("^_^");

            using (WebSocketServer<EchoJob> server = new WebSocketServer<EchoJob>(address, jobInitializer))
            {
                Task _ = server.StartAsync();

                Console.WriteLine("Start web socket at http://127.0.0.1:8089/");

                Console.WriteLine("Enter exit to stop the server");

                while (true)
                {
                    string line = Console.ReadLine();

                    if (string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Start exiting, please wait a while...");

                        break;
                    }

                    Thread.Sleep(2000);
                }
            }

            Console.WriteLine("Exited.");
            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            Console.WriteLine("タスクの異常:\r\n{0}", e.Exception);
        }
    }
}
