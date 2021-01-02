using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class RunNextStepEvent : INotification
    {
        public RunNextStepEvent(string executionId, string completedStepId)
        {
            ExecutionId = executionId;
            CompletedStepId = completedStepId;
        }

        public RunNextStepEvent(string executionId)
        {
            ExecutionId = executionId;
        }

        public string ExecutionId { get; }
        public string CompletedStepId { get; }
    }
}
