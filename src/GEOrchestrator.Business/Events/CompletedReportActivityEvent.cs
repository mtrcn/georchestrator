using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class CompletedReportActivityEvent : INotification
    {
        public CompletedReportActivityEvent(string jobId, string stepExecutionId, CompletedReportActivity activity)
        {
            JobId = jobId;
            Activity = activity;
            StepExecutionId = stepExecutionId;
        }

        public string JobId { get; }
        public string StepExecutionId { get; }
        public CompletedReportActivity Activity { get; }
    }
}
