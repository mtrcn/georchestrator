using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories.Executions;
using GEOrchestrator.Business.Repositories.Objects;
using GEOrchestrator.Business.Repositories.Parameters;
using GEOrchestrator.Domain.Models.Parameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GEOrchestrator.Business.Services
{
    public class ParameterService : IParameterService
    {
        private readonly IExecutionRepository _executionRepository;
        private readonly IExecutionStepRepository _executionStepRepository;
        private readonly IObjectRepository _objectRepository;
        private readonly IParameterRepository _parameterRepository;

        public ParameterService(
            IExecutionRepositoryFactory executionRepositoryFactory, 
            IExecutionStepRepositoryFactory executionStepRepositoryFactory, 
            IObjectRepositoryFactory objectRepositoryFactory,
            IParameterRepositoryFactory parameterRepositoryFactory)
        {
            _executionRepository = executionRepositoryFactory.Create();
            _executionStepRepository = executionStepRepositoryFactory.Create();
            _objectRepository = objectRepositoryFactory.Create();
            _parameterRepository = parameterRepositoryFactory.Create();
        }

        public async Task<NextExecutionParameterResponse> GetNextExecutionParameterAsync(NextExecutionParameterRequest nextExecutionParameterRequest)
        {
            //Get parameters from execution step object
            var executionStep = await _executionStepRepository.GetByExecutionIdAndStepIdAsync(nextExecutionParameterRequest.ExecutionId, nextExecutionParameterRequest.StepId);

            //Get next parameter by marker
            var nextIndex = GetNextIndexFromMarker(nextExecutionParameterRequest.Marker);
            if (executionStep.Step.Inputs == null || executionStep.Step.Inputs.Parameters.Count == 0 || nextIndex + 1 > executionStep.Step.Inputs.Parameters.Count)
                return new NextExecutionParameterResponse();

            //Generate next marker
            var nextMarker = nextIndex + 1 < executionStep.Step.Inputs.Parameters.Count
                ? GetMarkerFromNextIndex(nextIndex + 1)
                : null;
            
            var parameter = executionStep.Step.Inputs.Parameters[nextIndex];
            if (!IsReferenceParameter(parameter.Value))
                return new NextExecutionParameterResponse
                {
                    Name = parameter.Name,
                    Content = parameter.Value,
                    Marker = nextMarker
                };


            var inputValue = parameter.Value;
            if (inputValue == "{{item}}") //iteration parameter
            {
                var execution = await _executionRepository.GetByIdAsync(executionStep.ExecutionId);
                return new NextExecutionParameterResponse
                {
                    Name = parameter.Name,
                    Content = execution.Iteration.ItemValue,
                    Marker = null
                };
            }

            var (stepId, parameterName) = ParseValue(inputValue);

            var executionParameter = await _parameterRepository.GetAsync(nextExecutionParameterRequest.WorkflowRunId, stepId, parameterName);
            var content = await _objectRepository.GetAsync(executionParameter.StoragePath);

            return new NextExecutionParameterResponse
            {
                Name = parameter.Name,
                Content = Encoding.UTF8.GetString(content),
                Marker = nextMarker
            };
        }

        public async Task<List<JToken>> GetParameterValuesAsync(string workflowRunId, string stepId, string name)
        {
            var executionParameter = await _parameterRepository.GetAsync(workflowRunId, stepId, name);
            var content = await _objectRepository.GetAsync(executionParameter.StoragePath);
            var contentAsString = Encoding.UTF8.GetString(content);
            var token = JToken.Parse(contentAsString);
            if (token is JArray array)
                return array.ToList();
            return new List<JToken>{ token };
        }

        public async Task<string> GetNextExecutionIterationMarker(string workflowRunId, string collectionValue, string lastMarkerKey)
        {
            var (stepId, parameterName) = ParseValue(collectionValue);
            var nextIndex = GetNextIndexFromMarker(lastMarkerKey);

            var executionParameter = await _parameterRepository.GetAsync(workflowRunId, stepId, parameterName);
            var content = await _objectRepository.GetAsync(executionParameter.StoragePath);

            var contentAsString = Encoding.UTF8.GetString(content);
            var token = JToken.Parse(contentAsString);
            if (token is JArray array && nextIndex < array.Count)
                return GetMarkerFromNextIndex(nextIndex + 1);

            return null;
        }

        public async Task SaveExecutionParameterAsync(SaveExecutionParameterRequest request)
        {
            var execution = await _executionRepository.GetByIdAsync(request.ExecutionId);
            var storagePath = Path.Join(request.WorkflowRunId, request.StepId, request.Name);

            var isIteration = execution.Iteration != null;

            if (isIteration && execution.Iteration.Index == 0)
            {
                var content = JsonConvert.SerializeObject(new[]
                {
                    request.Content
                });
                await _objectRepository.AddAsync(storagePath, Encoding.UTF8.GetBytes(content));
                await _parameterRepository.AddAsync(new Parameter
                {
                    WorkflowRunId = request.WorkflowRunId,
                    StepId = request.StepId,
                    Name = request.Name,
                    StoragePath = storagePath
                });
            } else if (isIteration && execution.Iteration.Index > 0)
            {
                var executionParameter = await _parameterRepository.GetAsync(request.WorkflowRunId, request.StepId, request.Name);
                var contentAsBytes = await _objectRepository.GetAsync(executionParameter.StoragePath);
                var content = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(contentAsBytes));
                content.Add(request.Content);
                await _objectRepository.AddAsync(storagePath, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content)));
            }
            else
            {
                await _objectRepository.AddAsync(storagePath, Encoding.UTF8.GetBytes(request.Content));
            }
        }

        private static (string stepId, string parameterName) ParseValue(string value)
        {
            var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\._]+)}}$";
            var regexResult = Regex.Match(value, valuePattern);
            return (regexResult.Groups[0].Value, regexResult.Groups[1].Value);
        }

        private bool IsReferenceParameter(string parameterValue)
        {
            var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\._]+)}}$";
            return Regex.Match(parameterValue, valuePattern).Success;
        }

        private int GetNextIndexFromMarker(string marker)
        {
            var nextIndex = 0;
            if (string.IsNullOrEmpty(marker))
                return nextIndex;

            var data = Convert.FromBase64String(marker);
            return int.Parse(Encoding.UTF8.GetString(data));
        }

        private string GetMarkerFromNextIndex(int nextIndex)
        {
            var marker = Encoding.UTF8.GetBytes(nextIndex.ToString());
            var result = Convert.ToBase64String(marker);
            return result;
        }
    }
}
