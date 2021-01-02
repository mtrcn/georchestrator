using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories.Tasks;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Tasks;
using GEOrchestrator.Domain.Models.Workflows;
using Task = GEOrchestrator.Domain.Models.Tasks.Task;

namespace GEOrchestrator.Business.Services
{
    public class WorkflowValidatorService : IWorkflowValidatorService
    {
        private readonly ITaskRepository _taskRepository;

        public WorkflowValidatorService(ITaskRepositoryFactory taskRepositoryFactory)
        {
            _taskRepository = taskRepositoryFactory.Create();
        }

        public async Task<(bool isValid, List<string> messages)> ValidateAsync(Workflow workflow)
        {
            var isValid = true;
            var validationMessages = new List<string>();

            if (string.IsNullOrEmpty(workflow.Name))
            {
                validationMessages.Add("Workflow Name cannot be empty.");
                isValid = false;
            }

            var namePattern = @"^[a-zA-Z0-9]{3,128}$";
            if (!Regex.Match(workflow.Name, namePattern).Success)
            {
                validationMessages.Add($"{workflow.Name}: Name doesn't not comply with requirements: Only consist of alpha-numeric characters and minimum 3 and maximum 128 characters length.");
                isValid = false;
            }

            //Validate Step Definitions
            var duplicatedSteps = GetAllStepIds(workflow.Steps).GroupBy(s => s.ToLowerInvariant()).Where(g => g.Count() > 1)
                .Select(g => g.Key).ToList();
            if (duplicatedSteps.Count() > 1)
            {
                validationMessages.Add($"{string.Join(',', duplicatedSteps)} : Step Ids are used more than once, it must be unique name.");
                isValid = false;
            }

            var outputParameterDefinitions = new List<WorkflowStepOutputParameter>();
            var outputArtifactDefinitions = new List<WorkflowStepOutputArtifact>();
            foreach (var stepDefinition in workflow.Steps)
            {
                var (isStepValid, messages) = await ValidateStepAsync(stepDefinition, outputParameterDefinitions, outputArtifactDefinitions);
                isValid = isValid && isStepValid;
                validationMessages.AddRange(messages);
                outputArtifactDefinitions.AddRange(stepDefinition.Outputs?.Artifacts ?? new List<WorkflowStepOutputArtifact>());
                outputParameterDefinitions.AddRange(stepDefinition.Outputs?.Parameters ?? new List<WorkflowStepOutputParameter>());
            }

            return (isValid, validationMessages);
        }

        private async Task<(bool isValid, List<string> messages)> ValidateStepAsync(WorkflowStep stepDefinition, List<WorkflowStepOutputParameter> outputParameterDefinitions, List<WorkflowStepOutputArtifact> outputArtifactDefinitions)
        {
            var isValid = true;
            var validationMessages = new List<string>();

            if (string.IsNullOrEmpty(stepDefinition.Id))
            {
                validationMessages.Add("Step Id cannot be empty.");
                isValid = false;
            }

            if (string.IsNullOrEmpty(stepDefinition.Task))
            {
                validationMessages.Add($"{stepDefinition.Id}: Task cannot be empty.");
                isValid = false;
            }

            var isLogicalStep = new[] {TaskType.Parallel, TaskType.Foreach}.Contains(stepDefinition.Task.ToLowerInvariant());
            Task task = null;
            try
            {
                if (!isLogicalStep)
                    task = await _taskRepository.GetByNameAsync(stepDefinition.Task);
            }
            catch(TaskNotFoundException)
            {
                isValid = false;
                validationMessages.Add($"{stepDefinition.Id}: Task not found in task repository.");
            }

            //Validate ForEach
            if (stepDefinition.Task.ToLowerInvariant() == TaskType.Foreach)
            {
                var hasCollection = stepDefinition.Inputs != null && (stepDefinition.Inputs.Parameters.Any(p => p.Name.ToLowerInvariant() == "collection") || stepDefinition.Inputs.Artifacts.Any(p => p.Name.ToLowerInvariant() == "collection"));
                if (!hasCollection)
                {
                    isValid = false;
                    validationMessages.Add($"{stepDefinition.Id}: ForEach step does not have any 'collection' input. It cannot be iterated.");
                }

                foreach (var iterationStep in stepDefinition.Iterate)
                {
                    var (isStepValid, messages) = await ValidateStepAsync(iterationStep, outputParameterDefinitions, outputArtifactDefinitions);
                    isValid = isValid && isStepValid;
                    validationMessages.AddRange(messages);
                    outputArtifactDefinitions.AddRange(stepDefinition.Outputs?.Artifacts ?? new List<WorkflowStepOutputArtifact>());
                    outputParameterDefinitions.AddRange(stepDefinition.Outputs?.Parameters ?? new List<WorkflowStepOutputParameter>());
                }
            }

            //Validate Parallel
            if (stepDefinition.Task.ToLowerInvariant() == TaskType.Parallel)
            {
                foreach (var parallelStep in stepDefinition.Branches.SelectMany(b => b))
                {
                    var (isStepValid, messages) = await ValidateStepAsync(parallelStep, outputParameterDefinitions, outputArtifactDefinitions);
                    isValid = isValid && isStepValid;
                    validationMessages.AddRange(messages);
                    outputArtifactDefinitions.AddRange(stepDefinition.Outputs?.Artifacts ?? new List<WorkflowStepOutputArtifact>());
                    outputParameterDefinitions.AddRange(stepDefinition.Outputs?.Parameters ?? new List<WorkflowStepOutputParameter>());
                }
            }

            //Validate Inputs
            if (task != null && stepDefinition.Inputs != null)
            {
                var (areInputsValid, inputsValidationMessages) = ValidateInputs(stepDefinition.Id, stepDefinition.Inputs, task.Inputs, outputParameterDefinitions, outputArtifactDefinitions);
                isValid = isValid && areInputsValid;
                validationMessages.AddRange(inputsValidationMessages);
            }

            

            //Validate Outputs
            if (task != null && stepDefinition.Outputs != null)
            {
                var (areOutputsValid, outputValidationMessages) = ValidateOutputs(stepDefinition.Id, stepDefinition.Outputs, task.Outputs);
                isValid = isValid && areOutputsValid;
                validationMessages.AddRange(outputValidationMessages);
            }

            return (isValid, validationMessages);
        }

        private (bool isValid, List<string> messages) ValidateInputs(string stepId, WorkflowStepInput inputDefinition, List<TaskInput> taskInputs, List<WorkflowStepOutputParameter> outputParameterDefinitions, List<WorkflowStepOutputArtifact> outputArtifactDefinitions)
        {
            var isValid = true;
            var validationMessages = new List<string>();

            //Validate Artifacts
            foreach (var artifactDefinition in inputDefinition.Artifacts)
            {
                if (string.IsNullOrEmpty(artifactDefinition.Name))
                {
                    validationMessages.Add($"{stepId}: Artifact name cannot be empty.");
                    isValid = false;
                }

                if (taskInputs.All(t => t.Name != artifactDefinition.Name && t.Type == TaskInputType.Artifact))
                {
                    validationMessages.Add($"{stepId}-{artifactDefinition.Name}: Artifact name cannot be found in the Task definition.");
                    isValid = false;
                }

                var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\._]+)}}|{{item}}$";
                if (!Regex.Match(artifactDefinition.Value, valuePattern).Success)
                {
                    validationMessages.Add($"{stepId}-{artifactDefinition.Name}: Value is not in correct format.");
                    isValid = false;
                }

                if (inputDefinition.Artifacts.Select(s => s.Name).Count(id => id == artifactDefinition.Name) > 1)
                {
                    validationMessages.Add($"{stepId}-{artifactDefinition.Name}: Name is used more than once in same step.");
                    isValid = false;
                }

                if (artifactDefinition.Value != "{{item}}" && !outputArtifactDefinitions.Select(a => a.Id).Contains(artifactDefinition.Value))
                {
                    validationMessages.Add($"{stepId}-{artifactDefinition.Name}: Referenced artifact name cannot be found in previous steps.");
                    isValid = false;
                }
            }

            //Validate Parameters
            foreach (var parameterDefinition in inputDefinition.Parameters)
            {
                if (string.IsNullOrEmpty(parameterDefinition.Name))
                {
                    validationMessages.Add($"{stepId}: Parameter name cannot be empty.");
                    isValid = false;
                }

                if (taskInputs.All(t => t.Name != parameterDefinition.Name && t.Type == TaskInputType.Parameter))
                {
                    validationMessages.Add($"{stepId}-{parameterDefinition.Name}: Parameter name cannot be found in the Task definition.");
                    isValid = false;
                }

                var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)}}$";
                if (Regex.Match(parameterDefinition.Value, valuePattern).Success)
                {
                    if (!outputParameterDefinitions.Select(a => a.Id).Contains(parameterDefinition.Value))
                    {
                        validationMessages.Add($"{stepId}-{parameterDefinition.Name}: Referenced parameter name cannot be found in previous steps.");
                        isValid = false;
                    }
                }

                if (inputDefinition.Parameters.Select(s => s.Name).Count(id => id == parameterDefinition.Name) > 1)
                {
                    validationMessages.Add($"{stepId}-{parameterDefinition.Name}: Name is used more than once in same step.");
                    isValid = false;
                }
            }

            return (isValid, validationMessages);
        }

        private (bool isValid, List<string> messages) ValidateOutputs(string stepId, WorkflowStepOutput outputDefinition, List<TaskOutput> taskOutputs)
        {
            var isValid = true;
            var validationMessages = new List<string>();

            //Validate Artifacts
            foreach (var artifactDefinition in outputDefinition.Artifacts)
            {
                if (string.IsNullOrEmpty(artifactDefinition.Name))
                {
                    validationMessages.Add($"{stepId}: Artifact name cannot be empty.");
                    isValid = false;
                }

                if (taskOutputs.All(t => t.Name != artifactDefinition.Name && t.Type == TaskInputType.Parameter))
                {
                    validationMessages.Add($"{stepId}-{artifactDefinition.Name}: Artifact name cannot be found in the Task definition.");
                    isValid = false;
                }

                artifactDefinition.Id = $"{{{{step.{stepId}.{artifactDefinition.Name}}}}}";
            }

            //Validate Parameters
            foreach (var parameterDefinition in outputDefinition.Parameters)
            {
                if (string.IsNullOrEmpty(parameterDefinition.Name))
                {
                    validationMessages.Add($"{stepId}: Parameter name cannot be empty.");
                    isValid = false;
                }

                if (taskOutputs.All(t => t.Name != parameterDefinition.Name && t.Type == TaskInputType.Parameter))
                {
                    validationMessages.Add($"{stepId}-{parameterDefinition.Name}: Parameter name cannot be found in the Task definition.");
                    isValid = false;
                }

                parameterDefinition.Id = $"{{{{step.{stepId}.{parameterDefinition.Name}}}}}";
            }

            return (isValid, validationMessages);
        }

        private List<string> GetAllStepIds(IEnumerable<WorkflowStep> steps)
        {
            var result = new List<string>();
            var workflowSteps = steps.ToList();
            result.AddRange(workflowSteps.Select(s => s.Id));
            result.AddRange(workflowSteps.Where(s => s.Branches.Count > 0).SelectMany(s => GetAllStepIds(s.Branches.SelectMany(b => b))));
            result.AddRange(workflowSteps.Where(s => s.Iterate.Count > 0).SelectMany(s => GetAllStepIds(s.Iterate)));
            return result;
        }
    }
}
