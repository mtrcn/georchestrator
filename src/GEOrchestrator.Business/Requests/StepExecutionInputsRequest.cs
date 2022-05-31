using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class StepExecutionInputsRequest : IRequest<StepExecutionInput>
    {
        public StepExecutionInputsRequest(string stepExecutionId)
        {
            StepExecutionId = stepExecutionId;
        }
        public string StepExecutionId { get; }
    }
}
