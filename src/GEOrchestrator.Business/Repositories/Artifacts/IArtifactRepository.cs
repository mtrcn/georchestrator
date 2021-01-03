using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Artifacts;

namespace GEOrchestrator.Business.Repositories.Artifacts
{
    public interface IArtifactRepository
    {
        Task<(string marker, Artifact artifact)> GetNextAsync(string workflowRunId, string stepId, string name, string lastMarker = null);
        Task AddAsync(Artifact artifact);
        Task<List<Artifact>> GetArtifactsByStepIdAndName(string workflowRunId, string stepId, string name);
    }
}
