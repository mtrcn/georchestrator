using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class CompleteExecutionEventHandler : INotificationHandler<CompleteExecutionEvent>
    {
        private readonly IExecutionService _executionService;
        private readonly IMediator _mediator;

        public CompleteExecutionEventHandler(IExecutionService executionService, IMediator mediator)
        {
            _executionService = executionService;
            _mediator = mediator;
        }

        public async Task Handle(CompleteExecutionEvent notification, CancellationToken cancellationToken)
        {
            var execution = await _executionService.GetExecutionById(notification.ExecutionId);
            //update status
            await _executionService.UpdateExecutionStatusAsync(notification.ExecutionId, notification.Status);

            var childExecutions = await _executionService.GetChildExecutionsByParentId(execution.ParentExecutionId);
            if (childExecutions.Count == 0 || childExecutions.All(e => e.Status == ExecutionStatus.Failed || e.Status == ExecutionStatus.Completed))
            {
                await _mediator.Publish(new RunNextStepEvent(execution.ParentExecutionId, execution.ParentStepId), cancellationToken);
                return;
            }

            var waitingExecutions = childExecutions.Where(e => e.Status == ExecutionStatus.Initiated).ToList();
            var runningExecutions = childExecutions.Where(e => e.Status == ExecutionStatus.Running).ToList();
            var index = 0;
            if (execution.Iteration != null && waitingExecutions.Count > 0 && runningExecutions.Count < execution.Iteration.MaxConcurrency)
            {
                for (var i = runningExecutions.Count; index < waitingExecutions.Count && i < execution.Iteration.MaxConcurrency; i++, index++)
                {
                    await _mediator.Publish(new RunNextStepEvent(waitingExecutions[0].Id), cancellationToken);
                    waitingExecutions.RemoveAt(0);
                }
            }
        }
    }
}
