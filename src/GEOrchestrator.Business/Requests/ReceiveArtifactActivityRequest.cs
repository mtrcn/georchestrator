using GEOrchestrator.Domain.Models.Activities;
using GEOrchestrator.Domain.Models.Artifacts;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class ReceiveArtifactActivityRequest : IRequest<NextExecutionArtifactResponse>
    {
        public ReceiveArtifactActivityRequest(string workflowRunId, string executionId, string stepId,  ReceiveArtifactActivity activity)
        {
            WorkflowRunId = workflowRunId;
            ExecutionId = executionId;
            StepId = stepId;
            Activity = activity;
        }

        public ReceiveArtifactActivity Activity { get; }
        public string WorkflowRunId { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
