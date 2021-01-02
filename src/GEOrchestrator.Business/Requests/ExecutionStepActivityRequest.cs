using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class ExecutionStepActivityRequest : IRequest<object>
    {
        public ExecutionStepActivityRequest(ExecutionStepActivity executionStepActivity)
        {
            ExecutionStepActivity = executionStepActivity;
        }

        public ExecutionStepActivity ExecutionStepActivity { get; }
    }
}
