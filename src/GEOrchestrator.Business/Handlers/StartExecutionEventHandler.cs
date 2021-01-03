using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
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
            var executionId = await _executionService.CreateExecution(notification.StartExecutionRequest);
            if (notification.StartExecutionRequest.Iteration != null)
            {
                var childExecutions = await _executionService.GetChildExecutionsByParentId(notification.StartExecutionRequest.ParentExecutionId);
                if (childExecutions.Count(e => e.Status == ExecutionStatus.Running) >= notification.StartExecutionRequest.Iteration.MaxConcurrency)
                    return;
            }
            await _mediator.Publish(new RunNextStepEvent(executionId), cancellationToken);
        }
    }
}
