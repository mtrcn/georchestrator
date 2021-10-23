using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public class StepExecutionService : IStepExecutionService
    {
        private readonly IParameterRepository _parameterRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IObjectRepository _objectRepository;
        private readonly IStepExecutionRepository _stepExecutionRepository;
        private readonly IStepExecutionMessageRepository _executionStepMessageRepository;
        private readonly IJobRepository _jobRepository;
        

        public StepExecutionService(
            IParameterRepositoryFactory parameterRepositoryFactory,
            IArtifactRepositoryFactory artifactRepositoryFactory,
            IObjectRepositoryFactory objectRepositoryFactory,
            IExecutionStepRepositoryFactory executionStepRepositoryFactory,
            IExecutionStepMessageRepositoryFactory executionStepMessageRepositoryFactory,
            IJobRepositoryFactory jobRepositoryFactory)
        {
           
            _parameterRepository = parameterRepositoryFactory.Create();
            _artifactRepository = artifactRepositoryFactory.Create();
            _objectRepository = objectRepositoryFactory.Create();
            _stepExecutionRepository = executionStepRepositoryFactory.Create();
            _executionStepMessageRepository = executionStepMessageRepositoryFactory.Create();
            _jobRepository = jobRepositoryFactory.Create();
            
        }

        public async Task<StepExecution> CreateAsync(StepExecution stepExecution)
        {
            return await _stepExecutionRepository.CreateAsync(stepExecution);
        }

        public async Task UpdateStatusAsync(string stepExecutionId, string status)
        {
            await _stepExecutionRepository.UpdateStatusAsync(stepExecutionId, status);
        }

        public async Task UpdateContainerAsync(string stepExecutionId, Container container)
        {
            await _stepExecutionRepository.UpdateContainerAsync(stepExecutionId, container);
        }

        public async Task AddMessageAsync(StepExecutionMessage executionStepMessage)
        {
            await _executionStepMessageRepository.AddAsync(executionStepMessage);
        }

        public Task<StepExecution> GetByIdAsync(string stepExecutionId)
        {
            return _stepExecutionRepository.GetByIdAsync(stepExecutionId);
        }

        public async Task<StepExecution> IncreaseCompletedIteration(StepExecution stepExecution)
        {
            if (stepExecution.TotalIteration > stepExecution.CompletedIteration)
                stepExecution.CompletedIteration++;
            await _stepExecutionRepository.UpdateCompletedIterationAsync(stepExecution.Id, stepExecution.CompletedIteration);
            return stepExecution;
        }

        public async Task<List<StepExecution>> GetChildren(string parentStepExecutionId)
        {
            return await _stepExecutionRepository.GetByParentId(parentStepExecutionId);
        }

        public async Task<int> CalculateTotalIterationCount(string jobId, string stepId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            var workflowStep = job.Workflow.FindWorkflowStep(stepId);
            var workflowParameterInputs = await _parameterRepository.GetByReference(job.Id, workflowStep.Iterate.Collection);
            if (workflowParameterInputs.Count > 0)
                return workflowParameterInputs.Count;
            var collectionReferenceArtifact = workflowStep.Iterate.Collection.ParseStepReferenceValue();
            var workflowCollectionInputs = await _artifactRepository.GetAsync(job.Id, collectionReferenceArtifact.stepId, collectionReferenceArtifact.parameterName);
            return workflowCollectionInputs.Count;
        }

        public async Task<StepExecutionInput> GenerateInputs(StepExecution stepExecution)
        {
            var job = await _jobRepository.GetByIdAsync(stepExecution.JobId);
            var workflowStep = job.Workflow.FindWorkflowStep(stepExecution.StepId);

            var result = new StepExecutionInput();

            foreach (var workflowStepInput in workflowStep.Inputs.Parameters)
            {
                var referenceType = workflowStepInput.Value.GetReferenceType();
                switch (referenceType.ToLowerInvariant())
                {
                    case "step":
                    case "input":
                        var workflowInputs = await _parameterRepository.GetByReference(job.Id, workflowStepInput.Value);
                        result.Parameters.Add(workflowStepInput.Name, workflowInputs.FirstOrDefault()?.Value);
                        break;
                    case "item":
                        var parentExecution = await _stepExecutionRepository.GetByIdAsync(stepExecution.ParentStepExecutionId);
                        var parentStep = job.Workflow.FindWorkflowStep(parentExecution.StepId);
                        var workflowCollectionInputs = await _parameterRepository.GetByReference(job.Id, parentStep.Iterate.Collection);
                        result.Parameters.Add(workflowStepInput.Name, workflowCollectionInputs[stepExecution.Iteration].Value);
                        break;
                    default:
                        result.Parameters.Add(workflowStepInput.Name, workflowStepInput.Value);
                        break;
                }
            }

            foreach (var workflowStepInput in workflowStep.Inputs.Artifacts)
            {
                var referenceType = workflowStepInput.Value.GetReferenceType();
                switch (referenceType.ToLowerInvariant())
                {
                    case "step":
                        var stepParameterName = workflowStepInput.Value.ParseStepReferenceValue();
                        var stepOutputs = await _artifactRepository.GetAsync(job.Id, stepParameterName.stepId, stepParameterName.parameterName);
                        var downloadUrl = await _objectRepository.GetSignedUrlForDownloadAsync(stepOutputs.FirstOrDefault()?.StoragePath);
                        result.Artifacts.Add(workflowStepInput.Name, downloadUrl);
                        break;
                    case "item":
                        var parentExecution = await _stepExecutionRepository.GetByIdAsync(stepExecution.ParentStepExecutionId);
                        var parentStep = job.Workflow.FindWorkflowStep(parentExecution.StepId);
                        var collectionReferenceArtifact = parentStep.Iterate.Collection.ParseStepReferenceValue();
                        var workflowCollectionInputs = await _artifactRepository.GetAsync(job.Id, collectionReferenceArtifact.stepId, collectionReferenceArtifact.parameterName);
                        var signedUrlForCollectionItemDownload = await _objectRepository.GetSignedUrlForDownloadAsync(workflowCollectionInputs[stepExecution.Iteration].StoragePath);
                        result.Artifacts.Add(workflowStepInput.Name, signedUrlForCollectionItemDownload);
                        break;
                }
            }

            return result;
        }
    }
}
