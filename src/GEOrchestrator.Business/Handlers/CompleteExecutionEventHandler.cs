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
    public class CompleteExecutionEventHandler : INotificationHandler<CompleteExecutionEvent>
    {
        private readonly IExecutionService _executionService;
        private readonly IArtifactService _artifactService;
        private readonly IParameterService _parameterService;
        private readonly IMediator _mediator;

        public CompleteExecutionEventHandler(IExecutionService executionService, IArtifactService artifactService, IParameterService parameterService, IMediator mediator)
        {
            _executionService = executionService;
            _artifactService = artifactService;
            _parameterService = parameterService;
            _mediator = mediator;
        }

        public async Task Handle(CompleteExecutionEvent notification, CancellationToken cancellationToken)
        {
            var execution = await _executionService.GetExecutionById(notification.ExecutionId);
            //update status
            await _executionService.UpdateExecutionStatusAsync(notification.ExecutionId, notification.Status);

            var iteration = execution.Iteration;
            if (iteration != null)
            {
                if (execution.Iteration.CollectionType == CollectionType.Artifact)
                {
                    iteration.Marker = await _artifactService.GetNextExecutionIterationMarker(execution.WorkflowRunId, iteration.CollectionValue, iteration.Marker);
                }

                if (execution.Iteration.CollectionType == CollectionType.Parameter)
                {
                    iteration.Marker = await _parameterService.GetNextExecutionIterationMarker(execution.WorkflowRunId, iteration.CollectionValue, iteration.Marker);
                }

                iteration.Index++;

                //continue to next iteration
                if (!string.IsNullOrEmpty(iteration.Marker))
                {
                    await _mediator.Publish(new StartExecutionEvent(new StartExecutionRequest
                    {
                        ParentExecutionId = execution.ParentExecutionId,
                        ParentStepId = execution.ParentStepId,
                        WorkflowName = execution.WorkflowName,
                        WorkflowVersion = execution.WorkflowVersion,
                        WorkflowRunId = execution.WorkflowRunId,
                        Iteration = iteration,
                        Steps = execution.Steps
                    }), cancellationToken);
                }
            }
            
            if (string.IsNullOrEmpty(iteration?.Marker))
            {
                //check if parentId is set
                if (!string.IsNullOrEmpty(execution.ParentExecutionId))
                {
                    var childExecutions = await _executionService.GetChildExecutionsByParentId(execution.ParentExecutionId);
                    //check if parent's all child are executed.
                    if (childExecutions.All(e => e.Status == ExecutionStatus.Failed || e.Status == ExecutionStatus.Completed))
                    {
                        await _mediator.Publish(new RunNextStepEvent(execution.ParentExecutionId, execution.ParentStepId), cancellationToken);
                    }
                }
            }
        }
    }
}
