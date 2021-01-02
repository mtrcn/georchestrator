using GEOrchestrator.Domain.Models.Activities;
using GEOrchestrator.Domain.Models.Parameters;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class ReceiveParameterActivityRequest : IRequest<NextExecutionParameterResponse>
    {
        public ReceiveParameterActivityRequest(string workflowRunId, string executionId, string stepId,  ReceiveParameterActivity activity)
        {
            WorkflowRunId = workflowRunId;
            ExecutionId = executionId;
            StepId = stepId;
            Activity = activity;
        }

        public ReceiveParameterActivity Activity { get; }
        public string WorkflowRunId { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
