using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Artifacts;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Handlers
{
    public class StepExecutionInputsRequestHandler : IRequestHandler<StepExecutionInputsRequest, StepExecutionInput>
    {
        private readonly IStepExecutionService _stepExecutionService;

        public StepExecutionInputsRequestHandler(IStepExecutionService stepExecutionService)
        {
            _stepExecutionService = stepExecutionService;
        }

        public async Task<StepExecutionInput> Handle(StepExecutionInputsRequest notification, CancellationToken cancellationToken)
        {
            var stepExecution = await _stepExecutionService.GetByIdAsync(notification.StepExecutionId);
            return await _stepExecutionService.GenerateInputs(stepExecution);
        }
    }
}
