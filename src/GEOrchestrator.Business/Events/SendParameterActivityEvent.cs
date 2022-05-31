using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendParameterActivityEvent : INotification
    {
        public SendParameterActivityEvent(string workflowRunId, string executionId, string stepId,  SendParameterActivity activity)
        {
            WorkflowRunId = workflowRunId;
            ExecutionId = executionId;
            StepId = stepId;
            Activity = activity;
        }

        public SendParameterActivity Activity { get; }
        public string WorkflowRunId { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
