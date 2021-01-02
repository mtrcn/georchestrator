using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendInformationMessageActivityEvent : INotification
    {
        public SendInformationMessageActivityEvent(string executionId, string stepId,  SendInformationMessageActivity activity)
        {
            ExecutionId = executionId;
            StepId = stepId;
            Activity = activity;
        }

        public SendInformationMessageActivity Activity { get; }
        public string ExecutionId { get; }
        public string StepId { get; }
    }
}
