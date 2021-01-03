using GEOrchestrator.Domain.Models.Workflows;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class RunForEachStepEvent : INotification
    {
        public RunForEachStepEvent(string executionId, WorkflowStep forEachStep)
        {
            ExecutionId = executionId;
            ForEachStep = forEachStep;
        }

        public string ExecutionId { get; }
        public WorkflowStep ForEachStep { get; }
    }
}
