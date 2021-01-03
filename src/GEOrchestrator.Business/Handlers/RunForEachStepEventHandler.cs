using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Business.Utils;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GEOrchestrator.Business.Handlers
{
    public class RunForEachStepEventHandler : INotificationHandler<RunForEachStepEvent>
    {
        private readonly IExecutionService _executionService;
        private readonly IArtifactService _artifactService;
        private readonly IParameterService _parameterService;
        private readonly IMediator _mediator;

        public RunForEachStepEventHandler(IExecutionService executionService, IArtifactService artifactService, IParameterService parameterService, IMediator mediator)
        {
            _executionService = executionService;
            _artifactService = artifactService;
            _parameterService = parameterService;
            _mediator = mediator;
        }

        public async Task Handle(RunForEachStepEvent notification, CancellationToken cancellationToken)
        {
            var execution = await _executionService.GetExecutionById(notification.ExecutionId);
            var step = notification.ForEachStep;
            var (stepId, name) = ValueParser.Parse(step.Iterate.Collection);

            var collectionType = CollectionType.Artifact;
            var artifactInputs = await _artifactService.GetArtifactsByStepIdAndName(execution.WorkflowRunId, stepId, name);
            var inputs = artifactInputs.Select(a => a.StoragePath).ToList();
            if (inputs.Count == 0)
            {
                var parameterValues = await _parameterService.GetParameterValuesAsync(execution.WorkflowRunId, stepId, name);
                inputs = parameterValues.Select(token => token.ToString(Formatting.None)).ToList();
                collectionType = CollectionType.Parameter;
            }

            if (inputs.Count == 0)
                throw new InvalidOperationException($"{step.Id}: Collection cannot be found neither in parameter nor artifact store.");

            var index = 0;
            foreach (var input in inputs)
            {
                await _mediator.Publish(new StartExecutionEvent(new CreateExecutionRequest
                {
                    ParentExecutionId = execution.Id,
                    ParentStepId = step.Id,
                    WorkflowName = execution.WorkflowName,
                    WorkflowVersion = execution.WorkflowVersion,
                    WorkflowRunId = execution.WorkflowRunId,
                    Iteration = new ExecutionIteration
                    {
                        MaxConcurrency = step.Iterate.MaxConcurrency,
                        Index = index++,
                        ItemType = collectionType,
                        ItemValue = input
                    },
                    Steps = step.Iterate.Steps
                }), cancellationToken);
            }
        }
    }
}
