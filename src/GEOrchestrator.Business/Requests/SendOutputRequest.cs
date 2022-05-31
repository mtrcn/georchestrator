using GEOrchestrator.Domain.Dtos;
using MediatR;

namespace GEOrchestrator.Business.Requests
{
    public class SendOutputRequest : IRequest<string>
    {
        public SendOutputRequest(string stepExecutionId, SendOutputActivityDto activity)
        {
            StepExecutionId = stepExecutionId;
            Activity = activity;
        }

        public SendOutputActivityDto Activity { get; }
        public string StepExecutionId { get; }
    }
}
