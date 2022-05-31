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
        private readonly IStepExecutionService _stepExecutionService;

        public SendInformationMessageActivityEventHandler(IStepExecutionService executionService)
        {
            _stepExecutionService = executionService;
        }

        public async Task Handle(SendInformationMessageActivityEvent request, CancellationToken cancellationToken)
        {
            await _stepExecutionService.AddMessageAsync(new StepExecutionMessage
            {
                StepExecutionId = request.StepExecutionId,
                Message = request.Activity.Message,
                SentOn = request.Activity.SentOn,
                Type = ExecutionStepMessageType.Information
            });
        }
    }
}
