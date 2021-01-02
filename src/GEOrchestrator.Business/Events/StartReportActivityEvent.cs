using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class StartReportActivityEvent : INotification
    {
        public StartReportActivityEvent(string executionId, string stepId, StartReportActivity activity)
        {
            Activity = activity;
            ExecutionId = executionId;
            StepId = stepId;
        }

        public StartReportActivity Activity { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
