using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class StartExecutionEvent : INotification
    {
        public StartExecutionEvent(StartExecutionRequest startExecutionRequest)
        {
            StartExecutionRequest = startExecutionRequest;
        }
        public StartExecutionRequest StartExecutionRequest { get; }
    }
}
