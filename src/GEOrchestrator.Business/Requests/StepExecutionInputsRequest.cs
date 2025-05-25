using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class StepExecutionInputsRequest : IRequest<StepExecutionInput>
    {
        public StepExecutionInputsRequest(string jobId, string stepExecutionId)
        {
            JobId = jobId;
            StepExecutionId = stepExecutionId;
        }

        public string JobId { get; }
        public string StepExecutionId { get; }
    }
}
