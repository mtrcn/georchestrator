using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Artifacts;

namespace GEOrchestrator.Business.Services
{
    public interface IArtifactService
    {
        Task<NextExecutionArtifactResponse> GetNextExecutionArtifactAsync(NextExecutionArtifactRequest nextExecutionArtifactRequest);
        Task<string> SaveExecutionArtifactAsync(SaveExecutionArtifactRequest request);
        Task<string> GetNextExecutionIterationMarker(string workflowRunId, string collectionValue, string lastMarkerKey);
    }
}