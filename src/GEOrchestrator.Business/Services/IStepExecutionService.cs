using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public interface IStepExecutionService
    {
        Task UpdateStatusAsync(string stepExecutionId, string status);
        Task AddMessageAsync(StepExecutionMessage executionStepMessage);
        Task<StepExecution> GetByIdAsync(string stepExecutionId);
        Task<StepExecution> CreateAsync(StepExecution stepExecution);
        Task<StepExecution> IncreaseCompletedIteration(StepExecution stepExecution);
        Task<List<StepExecution>> GetChildren(string parentStepExecutionId);
        Task<int> CalculateTotalIterationCount(string jobId, string stepId);
        Task<StepExecutionInput> GenerateInputs(StepExecution stepExecution);
        Task UpdateContainerAsync(string stepExecutionId, Container container);
    }
}