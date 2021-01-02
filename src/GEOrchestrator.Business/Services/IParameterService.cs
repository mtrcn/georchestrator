using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Parameters;

namespace GEOrchestrator.Business.Services
{
    public interface IParameterService
    {
        Task<NextExecutionParameterResponse> GetNextExecutionParameterAsync(NextExecutionParameterRequest nextExecutionParameterRequest);
        Task SaveExecutionParameterAsync(SaveExecutionParameterRequest saveExecutionParameterRequest);
        Task<string> GetNextExecutionIterationMarker(string workflowRunId, string collectionValue, string lastMarkerKey);
    }
}