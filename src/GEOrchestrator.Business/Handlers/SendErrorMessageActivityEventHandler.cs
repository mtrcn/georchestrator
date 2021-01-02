using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class SendErrorMessageActivityEventHandler : INotificationHandler<SendErrorMessageActivityEvent>
    {
        private readonly IExecutionService _executionService;

        public SendErrorMessageActivityEventHandler(IExecutionService executionService)
        {
            _executionService = executionService;
        }

        public async Task Handle(SendErrorMessageActivityEvent request, CancellationToken cancellationToken)
        {
            await _executionService.AddExecutionStepMessageAsync(new AddExecutionStepMessageRequest
            {
                ExecutionId = request.ExecutionId,
                StepId = request.StepId,
                Message = request.Activity.Message,
                SentOn = request.Activity.SentOn,
                Type = ExecutionStepMessageType.Error
            });
            await _executionService.UpdateExecutionStepStatusAsync(request.ExecutionId, request.StepId, ExecutionStatus.Failed);
        }
    }
}
