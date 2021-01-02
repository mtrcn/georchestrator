using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Function.Clients
{
    public interface IOrchestratorClient
    {
        Task<string> SendActivity(string apiUrl, ExecutionStepActivity activity);
    }
}