using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class StepExecutionEvent : INotification
    {
        public StepExecutionEvent(string stepExecutionId, string type, string payload)
        {
            StepExecutionId = stepExecutionId;
            Type = type;
            Payload = payload;
        }

        public string StepExecutionId { get; }
        public string Type { get; }
        public string Payload { get; }
    }
}
