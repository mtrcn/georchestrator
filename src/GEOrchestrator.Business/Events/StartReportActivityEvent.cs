using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class StartReportActivityEvent : INotification
    {
        public StartReportActivityEvent(string stepExecutionId, StartedReportActivity activity)
        {
            Activity = activity;
            StepExecutionId = stepExecutionId;
        }

        public StartedReportActivity Activity { get; }
        public string StepExecutionId { get; }
    }
}
