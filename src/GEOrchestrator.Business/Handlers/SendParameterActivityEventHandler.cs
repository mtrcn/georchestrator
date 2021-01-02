using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Parameters;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class SendParameterActivityEventHandler : INotificationHandler<SendParameterActivityEvent>
    {
        private readonly IParameterService _parameterService;

        public SendParameterActivityEventHandler(IParameterService parameterService)
        {
            _parameterService = parameterService;
        }

        public async Task Handle(SendParameterActivityEvent request, CancellationToken cancellationToken)
        {
            await _parameterService.SaveExecutionParameterAsync(new SaveExecutionParameterRequest {
                WorkflowRunId = request.WorkflowRunId,
                ExecutionId = request.ExecutionId,
                StepId = request.StepId,
                Name = request.Activity.Name,
                Content = request.Activity.Content
            });
        }
    }
}
