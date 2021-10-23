using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Repositories
{
    public interface IWorkflowRepository
    {
        Task RegisterAsync(Domain.Models.Workflows.Workflow workflow);
        Task<Domain.Models.Workflows.Workflow> GetByNameAsync(string workflowName);
        Task<List<Workflow>> GetAllAsync();
    }
}
