using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Parameters;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class ReceiveParameterActivityRequestHandler : IRequestHandler<ReceiveParameterActivityRequest, NextExecutionParameterResponse>
    {
        private readonly IParameterService _parameterService;

        public ReceiveParameterActivityRequestHandler(IParameterService parameterService)
        {
            _parameterService = parameterService;
        }

        public async Task<NextExecutionParameterResponse> Handle(ReceiveParameterActivityRequest notification, CancellationToken cancellationToken)
        {
            var response = await _parameterService.GetNextExecutionParameterAsync(new NextExecutionParameterRequest
            {
                WorkflowRunId = notification.WorkflowRunId,
                ExecutionId = notification.ExecutionId,
                StepId = notification.StepId,
                Marker = notification.Activity.Marker
            });
            return response;
        }
    }
}
