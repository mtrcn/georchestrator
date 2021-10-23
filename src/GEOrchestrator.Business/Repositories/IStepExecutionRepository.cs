using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Repositories
{
    public interface IStepExecutionRepository
    {
        Task<StepExecution> CreateAsync(StepExecution stepExecution);
        Task<StepExecution> GetAsync(string jobId, int iteration, string stepId);
        Task UpdateStatusAsync(string id, string status);
        Task<StepExecution> GetByIdAsync(string id);
        Task<List<StepExecution>> GetByParentId(string parentStepExecutionId);
        Task UpdateCompletedIterationAsync(string id, int completedIteration);
        Task UpdateContainerAsync(string id, Container container);
    }
}
