using GEOrchestrator.Domain.Models.Workflows;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class RunWorkflowEvent : INotification
    {
        public RunWorkflowEvent(Workflow workflow)
        {
            Workflow = workflow;
        }
        public Workflow Workflow { get; }
    }
}
