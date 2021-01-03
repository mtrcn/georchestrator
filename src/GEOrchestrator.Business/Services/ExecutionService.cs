using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Providers.ContainerProviders;
using GEOrchestrator.Business.Repositories.Executions;
using GEOrchestrator.Business.Repositories.Tasks;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Workflows;
using Microsoft.Extensions.Configuration;

namespace GEOrchestrator.Business.Services
{
    public class ExecutionService : IExecutionService
    {
        private readonly IContainerProvider _containerProvider;
        private readonly ITaskRepository _taskRepository;
        private readonly IExecutionStepRepository _executionStepRepository;
        private readonly IExecutionStepMessageRepository _executionStepMessageRepository;
        private readonly IExecutionRepository _executionRepository;
        private readonly Dictionary<string, string> _defaultEnvironmentVariables;

        public ExecutionService(
            IContainerProviderFactory containerProviderFactory, 
            ITaskRepositoryFactory taskRepositoryFactory, 
            IExecutionStepRepositoryFactory executionStepRepositoryFactory,
            IExecutionStepMessageRepositoryFactory executionStepMessageRepositoryFactory,
            IExecutionRepositoryFactory executionRepositoryFactory,
            IConfiguration configuration)
        {
            _containerProvider = containerProviderFactory.Create();
            _taskRepository = taskRepositoryFactory.Create();
            _executionStepRepository = executionStepRepositoryFactory.Create();
            _executionStepMessageRepository = executionStepMessageRepositoryFactory.Create();
            _executionRepository = executionRepositoryFactory.Create();
            _defaultEnvironmentVariables = new Dictionary<string, string>
            {
                {"INPUT_PARAMETERS_PATH", "/function/parameters/input/"},
                {"INPUT_ARTIFACTS_PATH", "/function/artifacts/input/"},
                {"OUTPUT_PARAMETERS_PATH", "/function/parameters/output/"},
                {"OUTPUT_ARTIFACTS_PATH", "/function/artifacts/output/"},
                {"ORCHESTRATOR_API_URL", configuration["ORCHESTRATOR_API_URL"]}
            };
        }

        public async Task RunStepAsync(string workflowRunId, string executionId, WorkflowStep workflowStep)
        {
            var task = await _taskRepository.GetByNameAsync(workflowStep.Task);
            var environmentVariables = new Dictionary<string, string>(_defaultEnvironmentVariables)
            {
                {"EXECUTION_ID", executionId},
                {"STEP_ID", workflowStep.Id}
            };
            var runContainerResponse = await _containerProvider.RunAsync(task.Image, environmentVariables);
            await _executionStepRepository.CreateAsync(new ExecutionStep
            {
                WorkflowRunId = workflowRunId,
                ExecutionId = executionId,
                StepId = workflowStep.Id,
                Status = ExecutionStatus.Initiated,
                Step = workflowStep,
                ContainerId = runContainerResponse.Id,
                ContainerDetails = runContainerResponse.Details
            });
        }

        public async Task StopStepAsync(string executionId, string stepId)
        {
            var executionStep = await _executionStepRepository.GetByExecutionIdAndStepIdAsync(executionId, stepId);
            await _containerProvider.RemoveAsync(executionStep.ContainerId);
        }

        public async Task<ExecutionStep> GetExecutionStepByExecutionIdAndStepId(string executionId, string stepId)
        {
            var executionStep = await _executionStepRepository.GetByExecutionIdAndStepIdAsync(executionId, stepId);
            return executionStep;
        }

        public async Task UpdateExecutionStepStatusAsync(string executionId, string stepId, string status)
        {
            var executionStep = await _executionStepRepository.GetByExecutionIdAndStepIdAsync(executionId, stepId);
            if (executionStep.Status != ExecutionStatus.Failed)
                await _executionStepRepository.UpdateStatusAsync(executionId, stepId, status);
        }

        public async Task UpdateExecutionStatusAsync(string executionId, string status)
        {
            var execution = await _executionRepository.GetByIdAsync(executionId);
            if (execution.Status != ExecutionStatus.Failed)
                await _executionRepository.UpdateStatus(executionId, status);
        }

        public async Task AddExecutionStepMessageAsync(AddExecutionStepMessageRequest addExecutionStepMessageRequest)
        {
            await _executionStepMessageRepository.AddAsync(new ExecutionStepMessage
            {
                ExecutionId = addExecutionStepMessageRequest.ExecutionId,
                StepId = addExecutionStepMessageRequest.StepId,
                Message = addExecutionStepMessageRequest.Message,
                SentOn = addExecutionStepMessageRequest.SentOn,
                Type = addExecutionStepMessageRequest.Type
            });
        }

        public async Task<List<Execution>> GetChildExecutionsByParentId(string parentId)
        {
            var result = await _executionRepository.GetByParentId(parentId);
            return result.ToList();
        }

        public async Task<string> CreateExecution(CreateExecutionRequest request)
        {
            var execution = new Execution
            {
                WorkflowRunId = request.WorkflowRunId,
                Id = Guid.NewGuid().ToString(),
                ParentExecutionId = request.ParentExecutionId,
                ParentStepId = request.ParentStepId,
                StartedOn = DateTime.UtcNow,
                Steps = request.Steps,
                WorkflowName = request.WorkflowName,
                WorkflowVersion = request.WorkflowVersion,
                Iteration = request.Iteration,
                Status = ExecutionStatus.Initiated
            };
            await _executionRepository.Create(execution);
            return execution.Id;
        }

        public async Task<Execution> GetExecutionById(string executionId)
        {
            var execution = await _executionRepository.GetByIdAsync(executionId);
            return execution;
        }

        public WorkflowStep GetNextStep(Execution execution, string completedStepId)
        {
            var stepIndex = execution.Steps.FindIndex(s => s.Id == completedStepId);
            return stepIndex < execution.Steps.Count - 1 ? execution.Steps[stepIndex + 1] : null;
        }
    }
}
