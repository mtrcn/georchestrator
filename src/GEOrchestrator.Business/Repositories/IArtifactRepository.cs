using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Artifacts;

namespace GEOrchestrator.Business.Repositories
{
    public interface IArtifactRepository
    {
        Task AddAsync(Artifact artifact);
        Task<List<Artifact>> GetAsync(string jobId, string stepId, string name);
    }
}
