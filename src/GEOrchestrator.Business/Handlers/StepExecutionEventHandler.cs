using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Activities;
using MediatR;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class StepExecutionEventHandler : INotificationHandler<StepExecutionEvent>
    {
        private readonly IMediator _mediator;
        private readonly IStepExecutionService _stepExecutionService;

        public StepExecutionEventHandler(IMediator mediator, IStepExecutionService stepExecutionService)
        {
            _mediator = mediator;
            _stepExecutionService = stepExecutionService;
        }

        public async Task Handle(StepExecutionEvent notification, CancellationToken cancellationToken)
        {
            var payload = FromBase64(notification.Payload);
            var stepExecution = await _stepExecutionService.GetByIdAsync(notification.StepExecutionId);

            switch (notification.Type)
            {
                case nameof(StartedReportActivity):
                    await _mediator.Publish(new StartReportActivityEvent(stepExecution.Id, JsonSerializer.Deserialize<StartedReportActivity>(payload)), cancellationToken);
                    break;
                case nameof(InformationMessageActivity):
                    await _mediator.Publish(new SendInformationMessageActivityEvent(stepExecution.Id, JsonSerializer.Deserialize<InformationMessageActivity>(payload)), cancellationToken);
                    break;
                case nameof(ErrorMessageActivity):
                    await _mediator.Publish(new SendErrorMessageActivityEvent(stepExecution.Id, JsonSerializer.Deserialize<ErrorMessageActivity>(payload)), cancellationToken);
                    break;
                case nameof(CompletedReportActivity):
                    await _mediator.Publish(new CompletedReportActivityEvent(stepExecution.Id, JsonSerializer.Deserialize<CompletedReportActivity>(payload)), cancellationToken);
                    break;
            }
        }

        private static string FromBase64(string value)
        {
            var valueAsBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueAsBytes);
        }
    }
}
