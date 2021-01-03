using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Parameters;
using Newtonsoft.Json.Linq;

namespace GEOrchestrator.Business.Services
{
    public interface IParameterService
    {
        Task<NextExecutionParameterResponse> GetNextExecutionParameterAsync(NextExecutionParameterRequest nextExecutionParameterRequest);
        Task SaveExecutionParameterAsync(SaveExecutionParameterRequest saveExecutionParameterRequest);
        Task<string> GetNextExecutionIterationMarker(string workflowRunId, string collectionValue, string lastMarkerKey);
        Task<List<JToken>> GetParameterValuesAsync(string workflowRunId, string stepId, string name);
    }
}