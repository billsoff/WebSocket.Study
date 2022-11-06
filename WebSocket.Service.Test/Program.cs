﻿using System;
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
                server.Ready += OnServerReady;
                server.Fault += OnServerFault;
                server.Stopped += OnServerStopped;

                server.JobCreated += OnServerJobCreated;
                server.JobRemoved += OnServerJobRemoved;

                server.JobTermited += OnServerJobTermited;
                server.JobFault += OnServerJobFault;

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
            Console.WriteLine("Listening at \"{0}\"", ((WebSocketServer<EchoJob>)sender).ListeningAddress);
        }

        private static void OnServerStopped(object sender, EventArgs e)
        {
            Console.WriteLine("Server stopped.");
        }

        private static void OnServerFault(object sender, WebSocketServerFaultEventArgs e)
        {
            Console.WriteLine("Server startup failed:\r\n{0}", e.Exception);
        }

        private static void OnServerJobCreated(object sender, JobEventArgs<EchoJob> e)
        {
            Console.WriteLine("Job created (ID: {0}), total active jobs' count: {1}", e.Job.Id, e.ActiveJobs.Count);
            OutputJobStatus(e.Job);
        }

        private static void OnServerJobRemoved(object sender, JobEventArgs<EchoJob> e)
        {
            Console.WriteLine("Job removed (ID: {0}), total active jobs' count: {1}", e.Job.Id, e.ActiveJobs.Count);
            OutputJobStatus(e.Job);
        }

        private static void OnServerJobTermited(object sender, JobEventArgs<EchoJob> e)
        {
            Console.WriteLine("Job terminated (ID: {0}), total active jobs' count: {1}", e.Job.Id, e.ActiveJobs.Count);
            OutputJobStatus(e.Job);
        }

        private static void OnServerJobFault(object sender, JobFaultEventArgs<EchoJob> e)
        {
            Console.WriteLine(
                    "Job fault (ID: {0}), total active jobs' count: {1}\r\n{2}",
                    e.Job.Id,
                    e.ActiveJobs.Count,
                    e.Exception
                );
            OutputJobStatus(e.Job);
        }

        private static void OutputJobStatus(Job job)
        {
            Console.WriteLine("Job (ID: {0}) state: {1}", job.Id, job.WebSocketState);
            Console.WriteLine("Job (ID: {0}) close status: {1}", job.Id, job.WebSocketCloseStatus);
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            Console.WriteLine("タスクの異常:\r\n{0}", e.Exception);
        }
    }
}
