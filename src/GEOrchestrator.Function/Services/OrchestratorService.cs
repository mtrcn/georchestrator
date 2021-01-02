using Dawn;
using GEOrchestrator.Function.Clients;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Activities;
using GEOrchestrator.Domain.Models.Artifacts;
using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Parameters;

namespace GEOrchestrator.Function.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IOrchestratorClient _orchestratorClient;
        private readonly IHttpFileClient _httpFileClient;
        private readonly string _apiUrl;
        private readonly string _executionId;
        private readonly string _stepId;
        private readonly string _outputParametersPath;
        private readonly string _outputArtifactsPath;
        private readonly string _inputParametersPath;
        private readonly string _inputArtifactsPath;

        public OrchestratorService(IOrchestratorClient orchestratorClient, IHttpFileClient httpFileClient, IConfiguration configuration)
        {
            _orchestratorClient = orchestratorClient;
            _httpFileClient = httpFileClient;

            _apiUrl = configuration["ORCHESTRATOR_API_URL"];
            Guard.Argument(_apiUrl, nameof(_apiUrl)).NotNull().NotEmpty();

            _executionId = configuration["EXECUTION_ID"];
            Guard.Argument(_executionId, nameof(_executionId)).NotNull().NotEmpty();

            _stepId = configuration["STEP_ID"];
            Guard.Argument(_stepId, nameof(_stepId)).NotNull().NotEmpty();

            _outputParametersPath = configuration["OUTPUT_PARAMETERS_PATH"];
            Guard.Argument(_outputParametersPath, nameof(_outputParametersPath)).NotNull().NotEmpty();

            _outputArtifactsPath = configuration["OUTPUT_ARTIFACTS_PATH"];
            Guard.Argument(_outputArtifactsPath, nameof(_outputArtifactsPath)).NotNull().NotEmpty();

            _inputParametersPath = configuration["INPUT_PARAMETERS_PATH"];
            Guard.Argument(_inputParametersPath, nameof(_inputParametersPath)).NotNull().NotEmpty();

            _inputArtifactsPath = configuration["INPUT_ARTIFACTS_PATH"];
            Guard.Argument(_inputArtifactsPath, nameof(_inputArtifactsPath)).NotNull().NotEmpty();
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
            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityPayload = ToBase64(JsonConvert.SerializeObject(new StartReportActivity
                {
                    StartOn = startTime
                })),
                ActivityType = nameof(StartReportActivity)
            };


            await _orchestratorClient.SendActivity(_apiUrl, activity);
        }

        public async Task SendErrorMessageAsync(string message)
        {
            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityPayload = ToBase64(JsonConvert.SerializeObject(new SendErrorMessageActivity
                {
                    Message = message,
                    SentOn = DateTime.UtcNow
                })),
                ActivityType = nameof(SendErrorMessageActivity)
            };

            await _orchestratorClient.SendActivity(_apiUrl, activity);
        }

        public async Task SendInformationMessageAsync(string message)
        {
            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityPayload = ToBase64(JsonConvert.SerializeObject(new SendInformationMessageActivity
                {
                    Message = message,
                    SentOn = DateTime.UtcNow
                })),
                ActivityType = nameof(SendInformationMessageActivity)
            };

            await _orchestratorClient.SendActivity(_apiUrl, activity);
        }

        public async Task ReceiveParametersAsync()
        {
            var marker = string.Empty;
            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityType = nameof(ReceiveParameterActivity)
            };
            do
            {
                activity.ActivityPayload = ToBase64(JsonConvert.SerializeObject(new ReceiveParameterActivity
                {
                    Marker = marker
                }));
                var response = await _orchestratorClient.SendActivity(_apiUrl, activity);
                var executionParameterResponse = JsonConvert.DeserializeObject<NextExecutionParameterResponse>(response);
                if (!string.IsNullOrEmpty(executionParameterResponse.Content))
                {
                    await File.WriteAllTextAsync(Path.Join(_inputParametersPath, executionParameterResponse.Name), executionParameterResponse.Content);
                }
                marker = executionParameterResponse.Marker;
            } while (!string.IsNullOrEmpty(marker));
        }

        public async Task ReceiveArtifactsAsync()
        {
            var marker = string.Empty;
            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityType = nameof(ReceiveArtifactActivity)
            };
            do
            {
                activity.ActivityPayload = ToBase64(JsonConvert.SerializeObject(new ReceiveArtifactActivity
                {
                    Marker = marker
                }));
                var response = await _orchestratorClient.SendActivity(_apiUrl, activity);
                var executionArtifactResponse = JsonConvert.DeserializeObject<NextExecutionArtifactResponse>(response);
                if (!string.IsNullOrEmpty(executionArtifactResponse.Url))
                {
                    var path = Path.Join(_inputArtifactsPath, executionArtifactResponse.RelativePath);
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    await _httpFileClient.DownloadFile(executionArtifactResponse.Url, path);
                }
                marker = executionArtifactResponse.Marker;
            } while (!string.IsNullOrEmpty(marker));
        }

        public async Task SendOutputParametersAsync()
        {
            if (!File.Exists(_outputParametersPath))
            {
                return;
            }

            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityType = nameof(SendParameterActivity)
            };

            foreach (var file in Directory.GetFiles(_outputParametersPath))
            {
                activity.ActivityPayload = ToBase64(JsonConvert.SerializeObject(new SendParameterActivity
                {
                    Name = Path.GetFileName(file),
                    Content = await File.ReadAllTextAsync(file)
                }));
                await _orchestratorClient.SendActivity(_apiUrl, activity);
            }
        }

        public async Task SendOutputArtifactsAsync()
        {
            if (!Directory.Exists(_outputArtifactsPath))
            {
                return;
            }

            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityType = nameof(SendArtifactActivity)
            };

            foreach (var folder in Directory.GetDirectories(_outputArtifactsPath))
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    var relativePath = file.Replace(_outputArtifactsPath, string.Empty);
                    activity.ActivityPayload = ToBase64(JsonConvert.SerializeObject(new SendArtifactActivity
                    {
                        RelativePath = relativePath
                    }));
                    var uploadUrl = await _orchestratorClient.SendActivity(_apiUrl, activity);
                    await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    await _httpFileClient.UploadFile(uploadUrl, stream);
                }
            }

            foreach (var file in Directory.GetFiles(_outputArtifactsPath))
            {
                var relativePath = file.Replace(_outputArtifactsPath, string.Empty);
                activity.ActivityPayload = ToBase64(JsonConvert.SerializeObject(new SendArtifactActivity
                {
                    RelativePath = relativePath
                }));
                var uploadUrl = await _orchestratorClient.SendActivity(_apiUrl, activity);
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                await _httpFileClient.UploadFile(uploadUrl, stream);
            }
        }

        public async Task ReportCompletedAsync(DateTime completedTime)
        {
            var activity = new ExecutionStepActivity
            {
                ExecutionId = _executionId,
                StepId = _stepId,
                ActivityPayload = ToBase64(JsonConvert.SerializeObject(new CompletedReportActivity
                {
                    CompletedOn = completedTime
                })),
                ActivityType = nameof(CompletedReportActivity)
            };

            await _orchestratorClient.SendActivity(_apiUrl, activity);
        }

        private static string ToBase64(string value)
        {
            var valueAsBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueAsBytes);
        }
    }
}
