using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Services
{
    public interface IWorkflowService
    {
        Task<List<string>> Register(Workflow workflow);
        Task<Workflow> GetWorkflowByName(string workflowName);
        Task<List<Workflow>> GetAllAsync();
    }
}