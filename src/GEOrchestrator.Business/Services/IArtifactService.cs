using GEOrchestrator.Domain.Models.Artifacts;
using GEOrchestrator.Domain.Models.Executions;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public interface IArtifactService
    {
        Task<string> SaveArtifactAsync(Artifact artifact);
    }
}