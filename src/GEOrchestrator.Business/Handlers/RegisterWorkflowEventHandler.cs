using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Workflows;
using MediatR;
using SharpYaml.Serialization;

namespace GEOrchestrator.Business.Handlers
{
    public class RegisterWorkflowEventHandler : INotificationHandler<RegisterWorkflowEvent>
    {
        private readonly IWorkflowService _workflowService;

        public RegisterWorkflowEventHandler(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        public async Task Handle(RegisterWorkflowEvent notification, CancellationToken cancellationToken)
        {
            var serializer = new Serializer();
            var workflow = serializer.DeserializeInto(notification.WorkflowDefinition, new Workflow());
            await _workflowService.Register(workflow);
        }
    }
}
