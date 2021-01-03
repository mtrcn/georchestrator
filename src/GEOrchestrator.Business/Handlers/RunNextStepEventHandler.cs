using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class RunNextStepEventHandler : INotificationHandler<RunNextStepEvent>
    {
        private readonly IExecutionService _executionService;
        private readonly IMediator _mediator;

        public RunNextStepEventHandler(IExecutionService executionService, IMediator mediator)
        {
            _executionService = executionService;
            _mediator = mediator;
        }

        public async Task Handle(RunNextStepEvent notification, CancellationToken cancellationToken)
        {
            var execution = await _executionService.GetExecutionById(notification.ExecutionId);

            if (!string.IsNullOrEmpty(notification.CompletedStepId))
            {
                var isLogicalStep = new[] {TaskType.Parallel, TaskType.Foreach}.Contains(execution.Steps.First(s => s.Id == notification.CompletedStepId).Task.ToLowerInvariant());
                if (!isLogicalStep)
                {
                    var completedExecutionStep = await _executionService.GetExecutionStepByExecutionIdAndStepId(notification.ExecutionId, notification.CompletedStepId);
                    if (completedExecutionStep.Status == ExecutionStatus.Failed)
                    {
                        await _mediator.Publish(new CompleteExecutionEvent(execution.WorkflowRunId, execution.Id, ExecutionStatus.Failed), cancellationToken);
                        return;
                    }
                }
            }

            var nextStep = _executionService.GetNextStep(execution, notification.CompletedStepId);
            if (nextStep == null)
            {
                await _mediator.Publish(new CompleteExecutionEvent(execution.WorkflowRunId, execution.Id, ExecutionStatus.Completed), cancellationToken);
                return;
            }

            switch (nextStep.Task.ToLowerInvariant())
            {
                case TaskType.Parallel:
                    var tasks = nextStep.Branches.Select(branch => _mediator.Publish(new StartExecutionEvent(
                        new CreateExecutionRequest
                        {
                            ParentExecutionId = execution.Id,
                            ParentStepId = nextStep.Id,
                            WorkflowName = execution.WorkflowName,
                            WorkflowVersion = execution.WorkflowVersion,
                            WorkflowRunId = execution.WorkflowRunId,
                            Steps = branch
                        }), cancellationToken));
                    await Task.WhenAll(tasks);
                    break;
                case TaskType.Foreach:
                    await _mediator.Publish(new RunForEachStepEvent(execution.Id, nextStep), cancellationToken);
                    break;
                default:
                    await _executionService.RunStepAsync(execution.WorkflowRunId, execution.Id, nextStep);
                    break;
            }

            await _executionService.UpdateExecutionStatusAsync(execution.Id, ExecutionStatus.Running);
        }
    }
}
