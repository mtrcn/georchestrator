using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class RunWorkflowEvent : INotification
    {
        public RunWorkflowEvent(string workflowName)
        {
            WorkflowName = workflowName;
        }
        public string WorkflowName { get; }
    }
}
