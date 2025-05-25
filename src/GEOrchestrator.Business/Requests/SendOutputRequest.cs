using GEOrchestrator.Domain.Dtos;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class SendOutputRequest : IRequest<string>
    {
        public SendOutputRequest(string jobId, string stepExecutionId, SendOutputActivityDto activity)
        {
            JobId = jobId;
            StepExecutionId = stepExecutionId;
            Activity = activity;
        }

        public string JobId { get; }
        public string StepExecutionId { get; }
        public SendOutputActivityDto Activity { get; }
    }
}
