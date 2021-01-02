using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class SendArtifactActivityRequest : IRequest<string>
    {
        public SendArtifactActivityRequest(string workflowRunId, string executionId, string stepId,  SendArtifactActivity activity)
        {
            WorkflowRunId = workflowRunId;
            ExecutionId = executionId;
            StepId = stepId;
            Activity = activity;
        }

        public SendArtifactActivity Activity { get; }
        public string WorkflowRunId { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
