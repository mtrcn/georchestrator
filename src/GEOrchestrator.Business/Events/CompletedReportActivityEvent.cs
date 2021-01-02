using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class CompletedReportActivityEvent : INotification
    {
        public CompletedReportActivityEvent(string executionId, string stepId, CompletedReportActivity activity)
        {
            Activity = activity;
            ExecutionId = executionId;
            StepId = stepId;
        }

        public CompletedReportActivity Activity { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
