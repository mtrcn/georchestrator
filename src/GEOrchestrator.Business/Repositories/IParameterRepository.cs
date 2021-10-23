using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Parameters;

namespace GEOrchestrator.Business.Repositories
{
    public interface IParameterRepository
    {
        Task<List<Parameter>> GetAsync(string jobId, string name, string stepId = null);
        Task AddAsync(Parameter parameter);
        Task<List<Parameter>> GetByReference(string jobId, string referenceValue);
    }
}
