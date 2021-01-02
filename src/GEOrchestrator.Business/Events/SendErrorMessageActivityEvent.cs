using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendErrorMessageActivityEvent : INotification
    {
        public SendErrorMessageActivityEvent(string executionId, string stepId,  SendErrorMessageActivity activity)
        {
            ExecutionId = executionId;
            StepId = stepId;
            Activity = activity;
        }

        public SendErrorMessageActivity Activity { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
