using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Repositories
{
    public interface IStepExecutionMessageRepository
    {
        Task AddAsync(StepExecutionMessage executionMessage);
        Task<List<StepExecutionMessage>> GetByJobIdAsync(string jobId);
    }
}
