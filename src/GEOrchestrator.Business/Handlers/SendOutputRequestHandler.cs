using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Artifacts;
using GEOrchestrator.Domain.Models.Parameters;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class SendOutputRequestHandler : IRequestHandler<SendOutputRequest, string>
    {
        private readonly IParameterService _parameterService;
        private readonly IArtifactService _artifactService;
        private readonly IStepExecutionService _stepExecutionService;

        public SendOutputRequestHandler(IParameterService parameterService, IStepExecutionService stepExecutionService, IArtifactService artifactService)
        {
            _parameterService = parameterService;
            _stepExecutionService = stepExecutionService;
            _artifactService = artifactService;
        }

        public async Task<string> Handle(SendOutputRequest notification, CancellationToken cancellationToken)
        {
            var stepExecution = await _stepExecutionService.GetByIdAsync(notification.StepExecutionId);

            switch (notification.Activity.Type)
            {
                case TaskOutputType.Parameter:
                    await _parameterService.SaveParameterAsync(new Parameter
                    {
                        StepId = stepExecution.StepId,
                        Index = stepExecution.Iteration,
                        JobId = stepExecution.JobId,
                        Name = notification.Activity.Name,
                        Value = notification.Activity.Value
                    });
                    break;
                case TaskOutputType.Artifact:
                    return await _artifactService.SaveArtifactAsync(new Artifact
                    {
                        StepId = stepExecution.StepId,
                        Index = stepExecution.Iteration,
                        JobId = stepExecution.JobId,
                        Name = notification.Activity.Name
                    });
            }

            return string.Empty;
        }
    }
}
