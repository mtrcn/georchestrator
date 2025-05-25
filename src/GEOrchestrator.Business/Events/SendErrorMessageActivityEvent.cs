using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendErrorMessageActivityEvent : INotification
    {
        public SendErrorMessageActivityEvent(string jobId, string stepExecutionId,  ErrorMessageActivity activity)
        {
            JobId = jobId;
            StepExecutionId = stepExecutionId;
            Activity = activity;
        }

        public string JobId { get; }
        public string StepExecutionId { get; }
        public ErrorMessageActivity Activity { get; }
    }
}
