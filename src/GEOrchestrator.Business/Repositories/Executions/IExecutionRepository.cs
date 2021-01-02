using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Repositories.Executions
{
    public interface IExecutionRepository
    {
        Task Create(Execution execution);
        Task<Execution> GetByIdAsync(string id);
        Task UpdateStatus(string id, string status);
        Task<IEnumerable<Execution>> GetByParentId(string parentExecutionId);
    }
}
