using GEOrchestrator.Domain.Dtos;
using System.Threading.Tasks;

namespace GEOrchestrator.ContainerAgent.Clients
{
    public interface IOrchestratorClient
    {
        Task<string> SendActivityAsync(string apiUrl, StepExecutionActivityDto activity);
        Task<string> SendOutputAsync(string apiUrl, SendOutputActivityDto activity);
        Task<string> ReceiveInputsAsync(string apiUrl);
    }
}