using Dawn;
using GEOrchestrator.ContainerAgent.Clients;
using GEOrchestrator.Domain;
using GEOrchestrator.Domain.Dtos;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Activities;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GEOrchestrator.ContainerAgent.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IOrchestratorClient _orchestratorClient;
        private readonly IHttpFileClient _httpFileClient;
        private readonly string _apiUrl;
        private readonly string _outputParametersPath;
        private readonly string _outputArtifactsPath;
        private readonly string _inputParametersPath;
        private readonly string _inputArtifactsPath;

        public OrchestratorService(IOrchestratorClient orchestratorClient, IHttpFileClient httpFileClient, IConfiguration configuration)
        {
            _orchestratorClient = orchestratorClient;
            _httpFileClient = httpFileClient;

            var workflowApiUrl = configuration["WORKFLOW_API_URL"];
            Guard.Argument(workflowApiUrl, nameof(workflowApiUrl)).NotNull().NotEmpty();

            var stepExecutionId = configuration["STEP_EXECUTION_ID"];
            Guard.Argument(stepExecutionId, nameof(stepExecutionId)).NotNull().NotEmpty();

            _outputParametersPath = configuration["OUTPUT_PARAMETERS_PATH"];
            Guard.Argument(_outputParametersPath, nameof(_outputParametersPath)).NotNull().NotEmpty();

            _outputArtifactsPath = configuration["OUTPUT_ARTIFACTS_PATH"];
            Guard.Argument(_outputArtifactsPath, nameof(_outputArtifactsPath)).NotNull().NotEmpty();

            _inputParametersPath = configuration["INPUT_PARAMETERS_PATH"];
            Guard.Argument(_inputParametersPath, nameof(_inputParametersPath)).NotNull().NotEmpty();

            _inputArtifactsPath = configuration["INPUT_ARTIFACTS_PATH"];
            Guard.Argument(_inputArtifactsPath, nameof(_inputArtifactsPath)).NotNull().NotEmpty();

            _apiUrl = $"{workflowApiUrl}/stepexecutions/{stepExecutionId}";
        }

        public void CreateLockFile()
        {
            File.WriteAllBytes("/tmp/.lock", new byte[]{});
        }

        public void RemoveLockFile()
        {
            File.Delete("/tmp/.lock");
        }

        public void CreateDirectories()
        {
            if (!Directory.Exists(_outputParametersPath))
                Directory.CreateDirectory(_outputParametersPath);

            if (!Directory.Exists(_outputArtifactsPath))
                Directory.CreateDirectory(_outputArtifactsPath);
            
            if (!Directory.Exists(_inputParametersPath))
                Directory.CreateDirectory(_inputParametersPath);
            
            if (!Directory.Exists(_inputArtifactsPath))
                Directory.CreateDirectory(_inputArtifactsPath);
        }

        public async Task ReportStartAsync(DateTime startTime)
        {
            var activity = new StepExecutionActivityDto
            {
                Payload = ToBase64(JsonSerializer.Serialize(new StartedReportActivity
                {
                    StartOn = startTime
                }, GEOrchestratorJsonContext.Default.StartedReportActivity)),
                Type = nameof(StartedReportActivity)
            };


            await _orchestratorClient.SendActivityAsync($"{_apiUrl}/activities", activity);
        }

        public async Task SendErrorMessageAsync(string message)
        {
            var activity = new StepExecutionActivityDto
            {
                Payload = ToBase64(JsonSerializer.Serialize(new ErrorMessageActivity
                {
                    Message = message,
                    SentOn = DateTime.UtcNow
                }, GEOrchestratorJsonContext.Default.ErrorMessageActivity)),
                Type = nameof(ErrorMessageActivity)
            };

            await _orchestratorClient.SendActivityAsync($"{_apiUrl}/activities", activity);
        }

        public async Task SendInformationMessageAsync(string message)
        {
            var activity = new StepExecutionActivityDto
            {
                Payload = ToBase64(JsonSerializer.Serialize(new InformationMessageActivity
                {
                    Message = message,
                    SentOn = DateTime.UtcNow
                }, GEOrchestratorJsonContext.Default.InformationMessageActivity)),
                Type = nameof(InformationMessageActivity)
            };

            await _orchestratorClient.SendActivityAsync($"{_apiUrl}/activities", activity);
        }

        public async Task ReceiveInputsAsync()
        {
            var response = await _orchestratorClient.ReceiveInputsAsync($"{_apiUrl}/inputs");
            var stepExecutionInput = JsonSerializer.Deserialize(response, GEOrchestratorJsonContext.Default.StepExecutionInput);

            foreach (var parameter in stepExecutionInput.Parameters)
            {
                if (!string.IsNullOrEmpty(parameter.Value))
                {
                    await File.WriteAllTextAsync(Path.Join(_inputParametersPath, parameter.Key), parameter.Value);
                }
            }

            foreach (var artifact in stepExecutionInput.Artifacts)
            {
                if (!string.IsNullOrEmpty(artifact.Value))
                {
                    var path = Path.Join(_inputArtifactsPath, artifact.Key);
                    await _httpFileClient.DownloadFile(artifact.Value, path);
                }
            }

        }

        public async Task SendOutputParametersAsync()
        {
            if (!Directory.Exists(_outputParametersPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(_outputParametersPath))
            {
                await _orchestratorClient.SendOutputAsync($"{_apiUrl}/outputs", new SendOutputActivityDto
                {
                    Name = Path.GetFileName(file),
                    Value = await File.ReadAllTextAsync(file),
                    Type = TaskOutputType.Parameter
                });
            }
        }

        public async Task SendOutputArtifactsAsync()
        {
            if (!Directory.Exists(_outputArtifactsPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(_outputArtifactsPath))
            {
                var uploadUrl = await _orchestratorClient.SendOutputAsync($"{_apiUrl}/outputs", new SendOutputActivityDto
                {
                    Name = Path.GetFileName(file),
                    Type = TaskOutputType.Artifact
                });
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                await _httpFileClient.UploadFile(uploadUrl, stream);
            }
        }

        public async Task ReportCompletedAsync(DateTime completedTime)
        {
            var activity = new StepExecutionActivityDto
            {
                Payload = ToBase64(JsonSerializer.Serialize(new CompletedReportActivity
                {
                    CompletedOn = completedTime
                }, GEOrchestratorJsonContext.Default.CompletedReportActivity)),
                Type = nameof(CompletedReportActivity)
            };

            await _orchestratorClient.SendActivityAsync($"{_apiUrl}/activities", activity);
        }

        private static string ToBase64(string value)
        {
            var valueAsBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueAsBytes);
        }
    }
}
