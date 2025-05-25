using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class StepExecutionEvent : INotification
    {
        public StepExecutionEvent(string jobId, string stepExecutionId, string type, string payload)
        {
            JobId = jobId;
            StepExecutionId = stepExecutionId;
            Type = type;
            Payload = payload;
        }

        public string JobId { get; }
        public string StepExecutionId { get; }
        public string Type { get; }
        public string Payload { get; }
    }
}
