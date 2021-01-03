using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class RunWorkflowEventHandler : INotificationHandler<RunWorkflowEvent>
    {
        private readonly IWorkflowValidatorService _workflowValidatorService;
        private readonly IMediator _mediator;

        public RunWorkflowEventHandler(IWorkflowValidatorService workflowValidatorService, IMediator mediator)
        {
            _workflowValidatorService = workflowValidatorService;
            _mediator = mediator;
        }

        public async Task Handle(RunWorkflowEvent notification, CancellationToken cancellationToken)
        {
            var result = await _workflowValidatorService.ValidateAsync(notification.Workflow);

            if (!result.isValid)
                throw new WorkflowValidationException(string.Join(',', result.messages));

            if (result.isValid)
                await _mediator.Publish(new StartExecutionEvent(new CreateExecutionRequest
                {
                    WorkflowName = notification.Workflow.Name,
                    WorkflowVersion = notification.Workflow.Version,
                    WorkflowRunId = Guid.NewGuid().ToString(),
                    Steps = notification.Workflow.Steps
                }), cancellationToken);
        }

        
    }
}
