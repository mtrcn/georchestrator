using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Services
{
    public interface IWorkflowService
    {
        Task Register(Workflow workflow);
        Task<Workflow> GetWorkflowByName(string workflowName);
    }
}