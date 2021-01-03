using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Services
{
    public interface IExecutionService
    {
        Task RunStepAsync(string workflowRunId, string executionId, WorkflowStep workflowStep);
        Task UpdateExecutionStepStatusAsync(string executionId, string stepId, string status);
        Task<ExecutionStep> GetExecutionStepByExecutionIdAndStepId(string executionId, string stepId);
        Task<Execution> GetExecutionById(string executionId);
        WorkflowStep GetNextStep(Execution execution, string completedStepId);
        Task<string> CreateExecution(CreateExecutionRequest request);
        Task StopStepAsync(string executionId, string stepId);
        Task UpdateExecutionStatusAsync(string executionId, string status);
        Task<List<Execution>> GetChildExecutionsByParentId(string parentId);
        Task AddExecutionStepMessageAsync(AddExecutionStepMessageRequest addExecutionStepMessageRequest);
    }
}