using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;

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
                        new StartExecutionRequest
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
                    var collectionType = nextStep.Inputs.Parameters.Any(p => p.Name.ToLowerInvariant() == "collection") 
                        ? CollectionType.Parameter
                        : nextStep.Inputs.Artifacts.Any(p => p.Name.ToLowerInvariant() == "collection")
                            ? CollectionType.Artifact
                            : string.Empty;
                    if (string.IsNullOrEmpty(collectionType))
                        throw new InvalidOperationException($"An input named `collection` is not set for iteration step ({nextStep.Id}).");

                    var collectionValue = collectionType == CollectionType.Parameter
                        ? nextStep.Inputs.Parameters.Find(p => p.Name.ToLowerInvariant() == "collection").Value
                        : nextStep.Inputs.Artifacts.Find(p => p.Name.ToLowerInvariant() == "collection").Value;

                    await _mediator.Publish(new StartExecutionEvent(new StartExecutionRequest
                    {
                        ParentExecutionId = execution.Id,
                        ParentStepId = nextStep.Id,
                        WorkflowName = execution.WorkflowName,
                        WorkflowVersion = execution.WorkflowVersion,
                        WorkflowRunId = execution.WorkflowRunId,
                        Iteration = new ExecutionIteration
                        {
                            Index = 0,
                            CollectionType = collectionType,
                            CollectionValue = collectionValue,
                            Marker = null
                        },
                        Steps = nextStep.Iterate
                    }), cancellationToken);
                    break;
                default:
                    await _executionService.RunStepAsync(execution.WorkflowRunId, execution.Id, nextStep);
                    break;
            }

            await _executionService.UpdateExecutionStatusAsync(execution.Id, ExecutionStatus.Running);
        }
    }
}
