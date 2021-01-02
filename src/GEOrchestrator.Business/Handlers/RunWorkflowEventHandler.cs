using System;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class RunWorkflowEventHandler : INotificationHandler<RunWorkflowEvent>
    {
        private readonly IWorkflowService _workflowService;
        private readonly IMediator _mediator;

        public RunWorkflowEventHandler(IWorkflowService workflowService, IMediator mediator)
        {
            _workflowService = workflowService;
            _mediator = mediator;
        }

        public async Task Handle(RunWorkflowEvent notification, CancellationToken cancellationToken)
        {
            var workflow = await _workflowService.GetWorkflowByName(notification.WorkflowName);
            await _mediator.Publish(new StartExecutionEvent(new StartExecutionRequest
            {
                WorkflowName = workflow.Name,
                WorkflowVersion = workflow.Version,
                WorkflowRunId = Guid.NewGuid().ToString(),
                Steps = workflow.Steps
            }), cancellationToken);
        }

        
    }
}
