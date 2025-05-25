using System;
using System.Collections.Generic;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Activities;

namespace GEOrchestrator.Business.Handlers
{
    public class RunNextStepEventHandler : INotificationHandler<RunNextStepEvent>
    {
        private readonly IJobService _jobService;
        private readonly IStepExecutionService _stepExecutionService;
        private readonly IContainerService _containerService;
        private readonly IMediator _mediator;

        public RunNextStepEventHandler(
            IJobService jobService, 
            IStepExecutionService stepExecutionService, 
            IContainerService containerService,
            IMediator mediator)
        {
            _jobService = jobService;
            _containerService = containerService;
            _stepExecutionService = stepExecutionService;
            _mediator = mediator;
        }

        public async Task Handle(RunNextStepEvent notification, CancellationToken cancellationToken)
        {
            var job = await _jobService.GetJobAsync(notification.JobId);
            var nextStep = job.Workflow.FindWorkflowStep(notification.NextStepId);

            switch (nextStep.Task.ToLowerInvariant())
            {
                case TaskType.Parallel:
                    var parallelStepExecution = await _stepExecutionService.CreateAsync(new StepExecution
                    {
                        JobId = job.Id,
                        Iteration = notification.Iteration,
                        Status = ExecutionStatus.Running,
                        ParentStepExecutionId = notification.ParentStepExecutionId,
                        Task = nextStep.Task,
                        StepId = nextStep.Id
                    });
                    var parallelTasks = nextStep.Branches.Select(branch => _mediator.Publish(new RunNextStepEvent(notification.JobId, notification.Iteration, branch.First().Id, parallelStepExecution.Id), cancellationToken)).ToList();
                    await Task.WhenAll(parallelTasks);
                    break;
                case TaskType.Foreach:
                    var totalIterationCount = await _stepExecutionService.CalculateTotalIterationCount(job.Id, nextStep.Id);
                    var forEachExecution = await _stepExecutionService.CreateAsync(new StepExecution
                    {
                        JobId = job.Id,
                        Iteration = notification.Iteration,
                        TotalIteration = totalIterationCount,
                        Status = ExecutionStatus.Running,
                        ParentStepExecutionId = notification.ParentStepExecutionId,
                        Task = nextStep.Task,
                        StepId = nextStep.Id
                    });
                    if (totalIterationCount == 0)
                    {
                        await _mediator.Publish(new CompletedReportActivityEvent(job.Id, forEachExecution.Id, new CompletedReportActivity{CompletedOn = DateTime.UtcNow}), cancellationToken);
                        return;
                    }
                    var forEachTasks = new List<Task>();
                    for (var i = 0; i < Math.Min(nextStep.Iterate.MaxConcurrency, totalIterationCount); i++)
                    {
                        forEachTasks.Add(_mediator.Publish(new RunNextStepEvent(notification.JobId, i, nextStep.Iterate.Steps.First().Id, forEachExecution.Id), cancellationToken));
                    }
                    await Task.WhenAll(forEachTasks);
                    break;
                default:
                    var stepExecution = await _stepExecutionService.CreateAsync(new StepExecution
                    {
                        JobId = job.Id,
                        Iteration = notification.Iteration,
                        Status = ExecutionStatus.Created,
                        ParentStepExecutionId = notification.ParentStepExecutionId,
                        Task = nextStep.Task,
                        StepId = nextStep.Id
                    });
                    var container = await _containerService.RunAsync(stepExecution);
                    await _stepExecutionService.UpdateContainerAsync(stepExecution.Id, container);
                    break;
            }
        }
    }
}
