using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class CompleteJobEvent : INotification
    {
        public CompleteJobEvent(string jobId, string status, string message)
        {
            JobId = jobId;
            Status = status;
            Message = message;
        }

        public string Status { get; }
        public string Message { get; }
        public string JobId { get; }
    }
}
