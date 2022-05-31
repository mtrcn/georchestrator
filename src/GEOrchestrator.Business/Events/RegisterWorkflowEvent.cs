using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class RegisterWorkflowEvent : INotification
    {
        public RegisterWorkflowEvent(string workflowDefinition)
        {
            WorkflowDefinition = workflowDefinition;
        }

        public string WorkflowDefinition { get; }
    }
}
