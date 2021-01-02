using System.Threading.Tasks;

namespace GEOrchestrator.Business.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task RegisterAsync(Domain.Models.Workflows.Workflow workflow);
        Task<Domain.Models.Workflows.Workflow> GetByNameAsync(string workflowName);
    }
}
