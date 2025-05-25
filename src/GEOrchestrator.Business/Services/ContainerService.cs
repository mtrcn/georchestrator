using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Providers.ContainerProviders;
using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Services
{
    public class ContainerService : IContainerService
    {
        private readonly IContainerRepository _containerProvider;
        private readonly ITaskRepository _taskRepository;
        private readonly Dictionary<string, string> _defaultEnvironmentVariables;

        public ContainerService(
            IContainerProviderFactory containerProviderFactory,
            ITaskRepositoryFactory taskRepositoryFactory,
            IConfiguration configuration)
        {
            _containerProvider = containerProviderFactory.Create();
            _taskRepository = taskRepositoryFactory.Create();
            _defaultEnvironmentVariables = new Dictionary<string, string>
            {
                {"INPUT_PARAMETERS_PATH", "/function/parameters/input/"},
                {"INPUT_ARTIFACTS_PATH", "/function/artifacts/input/"},
                {"OUTPUT_PARAMETERS_PATH", "/function/parameters/output/"},
                {"OUTPUT_ARTIFACTS_PATH", "/function/artifacts/output/"},
                {"WORKFLOW_API_URL", configuration["WORKFLOW_API_URL"]}
            };
        }

        public async Task<Container> RunAsync(StepExecution stepExecution)
        {
            var task = await _taskRepository.GetByNameAsync(stepExecution.Task);
            var environmentVariables = new Dictionary<string, string>(_defaultEnvironmentVariables)
            {
                {"JOB_ID", stepExecution.JobId},
                {"STEP_EXECUTION_ID", stepExecution.Id}
            };
            var runContainerResponse = await _containerProvider.RunAsync(task.Image, environmentVariables);
            return runContainerResponse;
        }

        public async Task StopStepAsync(string containerId)
        {
            await _containerProvider.RemoveAsync(containerId);
        }
    }
}
