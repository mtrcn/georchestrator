using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class SendInformationMessageActivityEventHandler : INotificationHandler<SendInformationMessageActivityEvent>
    {
        private readonly IExecutionService _executionService;

        public SendInformationMessageActivityEventHandler(IExecutionService executionService)
        {
            _executionService = executionService;
        }

        public async Task Handle(SendInformationMessageActivityEvent request, CancellationToken cancellationToken)
        {
            await _executionService.AddExecutionStepMessageAsync(new AddExecutionStepMessageRequest
            {
                ExecutionId = request.ExecutionId,
                StepId = request.StepId,
                Message = request.Activity.Message,
                SentOn = request.Activity.SentOn,
                Type = ExecutionStepMessageType.Information
            });
        }
    }
}
