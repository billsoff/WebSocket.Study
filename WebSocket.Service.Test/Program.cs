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
            JobFactory jobInitializer = new JobFactory("^_^");

            using (WebSocketServer server = new WebSocketServer(address, jobInitializer))
            {
                server.Ready += OnServerReady;
                server.Fault += OnServerFault;
                server.Stopped += OnServerStopped;

                server.JobStart += OnServerJobStart;
                server.JobComplete += OnServerJobComplete;

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

        private static void OnServerReady(object sender, EventArgs e)
        {
            Console.WriteLine("Listening at \"{0}\"", ((WebSocketServer)sender).ListeningAddress);
        }

        private static void OnServerStopped(object sender, EventArgs e)
        {
            Console.WriteLine("Server stopped.");
        }

        private static void OnServerFault(object sender, WebSocketServerFaultEventArgs e)
        {
            Console.WriteLine("Server startup failed:\r\n{0}", e.Exception);
        }

        private static void OnServerJobStart(object sender, JobEventArgs e)
        {
            Console.WriteLine("Job start (ID: {0}), total active jobs' count: {1}", e.Job.Id, e.ActiveJobs.Count);
            OutputJobStatus(e.Job);
        }

        private static void OnServerJobComplete(object sender, JobCompleteEventArgs e)
        {
            Console.WriteLine("Job complete (ID: {0}), total active jobs' count: {1}, success: {2}", e.Job.Id, e.ActiveJobs.Count, e.Success);

            if (!e.Success)
            {
                Console.WriteLine(e.Exception);
            }

            OutputJobStatus(e.Job);
        }
        private static void OutputJobStatus(Job job)
        {
            Console.WriteLine("Job (ID: {0}) Is session active: {1}", job.Id, job.IsSocketSessionActive);
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            Console.WriteLine("タスクの異常:\r\n{0}", e.Exception);
        }
    }
}
