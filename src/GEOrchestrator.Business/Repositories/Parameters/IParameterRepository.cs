using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Parameters;

namespace GEOrchestrator.Business.Repositories.Parameters
{
    public interface IParameterRepository
    {
        Task<Parameter> GetAsync(string workflowRunId, string stepId, string name);
        Task AddAsync(Parameter parameter);
    }
}
