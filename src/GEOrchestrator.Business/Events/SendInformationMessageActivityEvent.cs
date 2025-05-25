using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SendInformationMessageActivityEvent : INotification
    {
        public SendInformationMessageActivityEvent(string jobId, string stepExecutionId,  InformationMessageActivity activity)
        {
            JobId = jobId;
            StepExecutionId = stepExecutionId;
            Activity = activity;
        }

        public string JobId { get; }
        public string StepExecutionId { get; }
        public InformationMessageActivity Activity { get; }
        
    }
}
