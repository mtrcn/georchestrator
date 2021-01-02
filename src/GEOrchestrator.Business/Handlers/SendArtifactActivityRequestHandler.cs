using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Artifacts;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class SendArtifactActivityRequestHandler : IRequestHandler<SendArtifactActivityRequest, string>
    {
        private readonly IArtifactService _artifactService;

        public SendArtifactActivityRequestHandler(IArtifactService artifactService)
        {
            _artifactService = artifactService;
        }

        public async Task<string> Handle(SendArtifactActivityRequest request, CancellationToken cancellationToken)
        {
            var response = await _artifactService.SaveExecutionArtifactAsync(new SaveExecutionArtifactRequest
            {
                WorkflowRunId = request.WorkflowRunId,
                ExecutionId = request.ExecutionId,
                StepId = request.StepId,
                RelativePath = request.Activity.RelativePath
            });
            return response;
        }
    }
}
