using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class RunNextStepEvent : INotification
    {
        public RunNextStepEvent(string jobId, int iteration, string nextStepId, string parentStepExecutionId = null)
        {
            JobId = jobId;
            Iteration = iteration;
            NextStepId = nextStepId;
            ParentStepExecutionId = parentStepExecutionId;
        }

        public string JobId { get; }
        public int Iteration { get; }
        public string NextStepId { get; }
        public string ParentStepExecutionId { get; }
    }
}
