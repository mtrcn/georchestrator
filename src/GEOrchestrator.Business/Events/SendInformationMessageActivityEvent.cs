using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendInformationMessageActivityEvent : INotification
    {
        public SendInformationMessageActivityEvent(string stepExecutionId,  InformationMessageActivity activity)
        {
            StepExecutionId = stepExecutionId;
            Activity = activity;
        }

        public string StepExecutionId { get; }
        public InformationMessageActivity Activity { get; }
        
    }
}
