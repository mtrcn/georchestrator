using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class StartExecutionEventHandler : INotificationHandler<StartExecutionEvent>
    {
        private readonly IExecutionService _executionService;
        private readonly IMediator _mediator;

        public StartExecutionEventHandler(IExecutionService executionService, IMediator mediator)
        {
            _executionService = executionService;
            _mediator = mediator;
        }

        public async Task Handle(StartExecutionEvent notification, CancellationToken cancellationToken)
        {
            var executionId = await _executionService.StartExecution(notification.StartExecutionRequest);
            await _mediator.Publish(new RunNextStepEvent(executionId), cancellationToken);
        }
    }
}
