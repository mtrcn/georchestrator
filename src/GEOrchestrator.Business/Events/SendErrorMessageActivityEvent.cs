using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendErrorMessageActivityEvent : INotification
    {
        public SendErrorMessageActivityEvent(string stepExecutionId,  ErrorMessageActivity activity)
        {
            StepExecutionId = stepExecutionId;
            Activity = activity;
        }

        public ErrorMessageActivity Activity { get; }
        public string StepExecutionId { get; }
    }
}
