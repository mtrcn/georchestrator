using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories.Artifacts;
using GEOrchestrator.Business.Repositories.Executions;
using GEOrchestrator.Business.Repositories.Objects;
using GEOrchestrator.Domain.Models.Artifacts;

namespace GEOrchestrator.Business.Services
{
    public class ArtifactService : IArtifactService
    {
        private readonly IExecutionStepRepository _executionStepRepository;
        private readonly IExecutionRepository _executionRepository;
        private readonly IObjectRepository _objectRepository;
        private readonly IArtifactRepository _artifactRepository;

        public ArtifactService(
            IExecutionRepositoryFactory executionRepositoryFactory, 
            IExecutionStepRepositoryFactory executionStepRepositoryFactory, 
            IObjectRepositoryFactory objectRepositoryFactory,
            IArtifactRepositoryFactory artifactRepositoryFactory)
        {
            _executionRepository = executionRepositoryFactory.Create();
            _executionStepRepository = executionStepRepositoryFactory.Create();
            _objectRepository = objectRepositoryFactory.Create();
            _artifactRepository = artifactRepositoryFactory.Create();
        }


        public async Task<NextExecutionArtifactResponse> GetNextExecutionArtifactAsync(NextExecutionArtifactRequest nextExecutionArtifactRequest)
        {
            var executionStep = await _executionStepRepository.GetByExecutionIdAndStepIdAsync(nextExecutionArtifactRequest.ExecutionId, nextExecutionArtifactRequest.StepId);

            var (lastArtifactName, lastMarkerKey) = DecodeMarker(nextExecutionArtifactRequest.Marker);

            var inputArtifact = string.IsNullOrEmpty(lastArtifactName) ?  
                executionStep.Step.Inputs?.Artifacts.FirstOrDefault() : 
                executionStep.Step.Inputs?.Artifacts.FirstOrDefault(a => a.Name == lastArtifactName);

            if (inputArtifact == null)
            {
                return new NextExecutionArtifactResponse();
            }

            var inputValue = inputArtifact.Value;
            if (inputValue == "{{item}}") //iteration artifact
            {
                var execution = await _executionRepository.GetByIdAsync(executionStep.ExecutionId);
                inputValue = execution.Iteration.CollectionValue;
                lastMarkerKey ??= execution.Iteration.Marker;
            }

            var (stepId, artifactName) = ParseValue(inputValue);

            var (nextMarkerKey, artifact) = await _artifactRepository.GetNextAsync(nextExecutionArtifactRequest.WorkflowRunId, stepId, artifactName, lastMarkerKey);
            
            nextMarkerKey = inputArtifact.Value == "{{item}}" ? null : nextMarkerKey;
            var nextInputArtifact = inputArtifact;
            if (string.IsNullOrEmpty(nextMarkerKey))
            {
                var inputArtifactIndex = executionStep.Step.Inputs.Artifacts.FindIndex(a => a.Name == inputArtifact.Name);
                nextInputArtifact = inputArtifactIndex + 1 < executionStep.Step.Inputs.Artifacts.Count
                    ? executionStep.Step.Inputs.Artifacts[inputArtifactIndex + 1]
                    : null;
            }

            var relativePath = artifact?.RelativePath;
            if (!string.IsNullOrEmpty(relativePath))
            {
                var relativePathDirectory = Path.GetDirectoryName(relativePath);
                var relativePathFile = Path.GetFileName(relativePath);

                if (inputArtifact.Value == "{{item}}")
                {
                    relativePath = inputArtifact.Name;
                }
                else if (!string.IsNullOrEmpty(relativePathDirectory))
                {
                    relativePath = Path.Join(artifactName, relativePathFile);
                }
                else
                {
                    relativePath = inputArtifact.Name;
                }
            }
           

            return new NextExecutionArtifactResponse
            {
                Marker = nextInputArtifact == null && string.IsNullOrEmpty(nextMarkerKey) ? null : EncodeMarker(nextInputArtifact?.Name, nextMarkerKey),
                Name = inputArtifact.Name,
                Url = artifact != null ? await _objectRepository.GetSignedUrlForDownloadAsync(artifact.StoragePath) : null,
                RelativePath = relativePath
            };
        }

        public async Task<string> GetNextExecutionIterationMarker(string workflowId, string collectionValue, string lastMarkerKey)
        {
            var (stepId, artifactName) = ParseValue(collectionValue);
            var result = await _artifactRepository.GetNextAsync(workflowId, stepId, artifactName, lastMarkerKey);
            return result.marker;
        }

        public async Task<string> SaveExecutionArtifactAsync(SaveExecutionArtifactRequest request)
        {
            var execution = await _executionRepository.GetByIdAsync(request.ExecutionId);

            var isIteration = execution.Iteration != null;

            var fileName = Path.GetFileName(request.RelativePath);
            if (string.IsNullOrEmpty(fileName))
                throw new InvalidOperationException($"{request.RelativePath} is not a correct file path.");
            
            var directoryPath = Path.GetDirectoryName(request.RelativePath);

            var separators = new[] {
                Path.DirectorySeparatorChar,  
                Path.AltDirectorySeparatorChar  
            };

            var artifactName = directoryPath.Split(separators)[0];

            if (string.IsNullOrEmpty(artifactName)) //is file
                artifactName = fileName;

            var storagePath = Path.Join(
                request.WorkflowRunId,
                request.StepId,
                directoryPath, 
                isIteration ? execution.Iteration.Index.ToString() : string.Empty, 
                fileName);

            storagePath = storagePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var relativePath = Path.Join(directoryPath, isIteration ? execution.Iteration.Index.ToString() : string.Empty, fileName);

            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            await _artifactRepository.AddAsync(new Artifact
            {
                WorkflowRunId = request.WorkflowRunId,
                Name = artifactName,
                RelativePath = relativePath,
                StoragePath = storagePath,
                StepId = request.StepId
            });

            var uploadUrl = await _objectRepository.GetSignedUrlForUploadAsync(storagePath);
            return uploadUrl;
        }

        private static (string stepId, string artifactName) ParseValue(string value)
        {
            var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\._]+)}}$";
            var regexResult = Regex.Match(value, valuePattern);
            return (regexResult.Groups[1].Value, regexResult.Groups[2].Value);
        }

        private string EncodeMarker(string artifactName, string lastKey)
        {
            var markerData = Encoding.UTF8.GetBytes($"{artifactName}#{lastKey}");
            return Convert.ToBase64String(markerData);
        }

        private (string artifactName, string lastKey) DecodeMarker(string marker)
        {
            if (string.IsNullOrEmpty(marker))
                return (null, null);

            var markerData = Convert.FromBase64String(marker);
            var markerContent = Encoding.UTF8.GetString(markerData);
            var markerComponents = markerContent.Split('#');
            return (markerComponents[0], markerComponents[1]);
        }
    }
}
