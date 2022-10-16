using System;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    internal sealed class EchoJob : Job
    {
        private string _message;

        internal string Suffix { get; set; }

        public override bool Recognize(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            Console.WriteLine(message);
            _message = message;

            return true;
        }

        public override async Task<JobExecutionStep> Execute()
        {
            await SendAsync($"Hello! {_message} {Suffix}");

            return JobExecutionStep.WaitNextReceive;
        }

        public override JobPolicyOnCompletion DeterminePolicyOnCompletion()
        {
            return JobPolicyOnCompletion.Termiante;
        }
    }
}
