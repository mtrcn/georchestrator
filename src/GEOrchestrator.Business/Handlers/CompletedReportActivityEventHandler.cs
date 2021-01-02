using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class CompletedReportActivityEventHandler : INotificationHandler<CompletedReportActivityEvent>
    {
        private readonly IExecutionService _executionService;
        private readonly IMediator _mediator;

        public CompletedReportActivityEventHandler(IExecutionService executionService, IMediator mediator)
        {
            _executionService = executionService;
            _mediator = mediator;
        }

        public async Task Handle(CompletedReportActivityEvent notification, CancellationToken cancellationToken)
        {
            await _executionService.UpdateExecutionStepStatusAsync(notification.ExecutionId, notification.StepId, ExecutionStatus.Completed);
            await _executionService.StopStepAsync(notification.ExecutionId, notification.StepId);
            await _mediator.Publish(new RunNextStepEvent(notification.ExecutionId, notification.StepId), cancellationToken);
        }
    }
}
