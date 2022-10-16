using System;
using System.Threading.Tasks;

namespace WebSocketService.Test
{
    internal sealed class EchoJob : Job
    {
        private string _message;

        public override bool Recognize(string message)
        {
            Console.WriteLine(message);
            _message = message;

            return true;
        }

        public override async Task<JobExecutionStep> Execute()
        {
            await SendAsync($"Hello! {_message}");

            return JobExecutionStep.WaitNextReceive;
        }

        public override JobPolicyOnCompletion DeterminePolicyOnCompletion()
        {
            return JobPolicyOnCompletion.Termiante;
        }
    }
}
