using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class CompletedReportActivityEvent : INotification
    {
        public CompletedReportActivityEvent(string stepExecutionId, CompletedReportActivity activity)
        {
            Activity = activity;
            StepExecutionId = stepExecutionId;
        }

        public CompletedReportActivity Activity { get; }
        public string StepExecutionId { get; }
    }
}
