using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Artifacts;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class ReceiveArtifactActivityRequestHandler : IRequestHandler<ReceiveArtifactActivityRequest, NextExecutionArtifactResponse>
    {
        private readonly IArtifactService _artifactService;

        public ReceiveArtifactActivityRequestHandler(IArtifactService artifactService)
        {
            _artifactService = artifactService;
        }

        public async Task<NextExecutionArtifactResponse> Handle(ReceiveArtifactActivityRequest notification, CancellationToken cancellationToken)
        {
            var response = await _artifactService.GetNextExecutionArtifactAsync(new NextExecutionArtifactRequest
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
