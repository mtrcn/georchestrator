using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class SendErrorMessageActivityEventHandler : INotificationHandler<SendErrorMessageActivityEvent>
    {
        private readonly IStepExecutionService _stepExecutionService;

        public SendErrorMessageActivityEventHandler(IStepExecutionService stepExecutionService)
        {
            _stepExecutionService = stepExecutionService;
        }

        public async Task Handle(SendErrorMessageActivityEvent request, CancellationToken cancellationToken)
        {
            await _stepExecutionService.AddMessageAsync(new StepExecutionMessage
            {
                StepExecutionId = request.StepExecutionId,
                Message = request.Activity.Message,
                SentOn = request.Activity.SentOn,
                Type = ExecutionStepMessageType.Error
            });

            await _stepExecutionService.UpdateStatusAsync(request.StepExecutionId, ExecutionStatus.Failed);
        }
    }
}
