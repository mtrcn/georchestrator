using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class CompleteExecutionEvent : INotification
    {
        public CompleteExecutionEvent(string workflowRunId, string executionId, string status)
        {
            WorkflowRunId = workflowRunId;
            ExecutionId = executionId;
            Status = status;
        }

        public string Status { get; }
        public string ExecutionId { get; }
        public string WorkflowRunId { get; }
    }
}
