using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class StartExecutionEvent : INotification
    {
        public StartExecutionEvent(CreateExecutionRequest startExecutionRequest)
        {
            StartExecutionRequest = startExecutionRequest;
        }
        public CreateExecutionRequest StartExecutionRequest { get; }
    }
}
