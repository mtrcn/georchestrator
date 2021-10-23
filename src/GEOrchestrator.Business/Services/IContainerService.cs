using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Services
{
    public interface IContainerService
    {
        Task<Container> RunAsync(StepExecution stepExecution);
        Task StopStepAsync(string containerId);
    }
}