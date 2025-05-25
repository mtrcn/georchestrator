using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Activities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class CompletedReportActivityEventHandler : INotificationHandler<CompletedReportActivityEvent>
    {
        private readonly IStepExecutionService _stepExecutionService;
        private readonly IContainerService _containerService;
        private readonly IJobService _jobService;
        private readonly IMediator _mediator;

        public CompletedReportActivityEventHandler(IStepExecutionService stepExecutionService, IContainerService containerService, IJobService jobService, IMediator mediator)
        {
            _stepExecutionService = stepExecutionService;
            _containerService = containerService;
            _mediator = mediator;
            _jobService = jobService;
        }

        public async Task Handle(CompletedReportActivityEvent notification, CancellationToken cancellationToken)
        {
            var completedExecutionStep = await _stepExecutionService.GetByIdAsync(notification.StepExecutionId);

            await _containerService.StopStepAsync(completedExecutionStep.ContainerId);

            //check if it is already marked as failed
            if (completedExecutionStep.Status == ExecutionStatus.Failed)
            {
                await _mediator.Publish(new CompleteJobEvent(completedExecutionStep.JobId, JobStatus.Failed, $"{completedExecutionStep.StepId} failed."), cancellationToken);
                return;
            }

            await _stepExecutionService.UpdateStatusAsync(notification.StepExecutionId, ExecutionStatus.Successful);

            var job = await _jobService.GetJobAsync(completedExecutionStep.JobId);

            if (job.Status == ExecutionStatus.Failed || job.Status == ExecutionStatus.Dismissed)
                return;

            var nextStep = job.Workflow.NextWorkflowStep(completedExecutionStep.StepId);

            if (nextStep == null)
            {
                await _mediator.Publish(new CompleteJobEvent(completedExecutionStep.JobId, JobStatus.Successful, "Completed successfully"), cancellationToken);
                return;
            }

            if (job.Status == ExecutionStatus.Dismissed || job.Status == ExecutionStatus.Failed)
                return;

            //Check if parent is also completed
            if (!string.IsNullOrEmpty(completedExecutionStep.ParentStepExecutionId))
            {
                var parentExecutionStep = await _stepExecutionService.GetByIdAsync(completedExecutionStep.ParentStepExecutionId);
                var childSteps = job.Workflow.GetChildren(parentExecutionStep.StepId);
                if (parentExecutionStep.Task.ToLowerInvariant() == TaskType.Foreach)
                {
                    var lastChild = childSteps.Last();
                    //if the completed step is the last step in the loop, then initiate new iteration
                    if (lastChild.Id == completedExecutionStep.StepId)
                    {
                        parentExecutionStep = await _stepExecutionService.IncreaseCompletedIteration(parentExecutionStep);
                        //if the loop is still not completed, then create a new iteration
                        if (parentExecutionStep.CompletedIteration < parentExecutionStep.TotalIteration)
                        {
                            var firstChild = childSteps.First();
                            await _mediator.Publish(new RunNextStepEvent(completedExecutionStep.JobId, parentExecutionStep.CompletedIteration+1, firstChild.Id, parentExecutionStep.Id), cancellationToken);
                            return;
                        }
                        await _mediator.Publish(new CompletedReportActivityEvent(job.Id, parentExecutionStep.Id, new CompletedReportActivity{ CompletedOn = DateTime.UtcNow}), cancellationToken);
                        return;
                    }
                }
                else if (parentExecutionStep.Task.ToLowerInvariant() == TaskType.Parallel)
                {
                    var childStepExecutions = await _stepExecutionService.GetChildren(parentExecutionStep.Id);
                    if (childStepExecutions.Count == childSteps.Count && childStepExecutions.All(c =>
                        c.Status == ExecutionStatus.Successful || c.Status == ExecutionStatus.Failed ||
                        c.Status == ExecutionStatus.Dismissed))
                    {
                        await _mediator.Publish(new CompletedReportActivityEvent(job.Id, parentExecutionStep.Id, new CompletedReportActivity { CompletedOn = DateTime.UtcNow }), cancellationToken);
                        return;
                    }
                    if (childSteps.Any(c => c.Id == nextStep.Id)) //if the next step is part of the parallel step, then run the next step
                    {
                        await _mediator.Publish(new RunNextStepEvent(completedExecutionStep.JobId, completedExecutionStep.Iteration, nextStep.Id, parentExecutionStep.Id), cancellationToken);
                        return;
                    }
                }
            }

            await _mediator.Publish(new RunNextStepEvent(completedExecutionStep.JobId, completedExecutionStep.Iteration, nextStep.Id, completedExecutionStep.ParentStepExecutionId), cancellationToken);
        }
    }
}
