using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Repositories.Executions
{
    public interface IExecutionStepRepository
    {
        Task CreateAsync(ExecutionStep executionStep);
        Task<ExecutionStep> GetByExecutionIdAndStepIdAsync(string executionId, string stepId);
        Task UpdateStatusAsync(string executionId, string stepId, string status);
    }
}
