using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Containers;

namespace GEOrchestrator.Business.Providers.ContainerProviders
{
    public interface IContainerProvider
    {
        Task<RunContainerResponse> RunAsync(string imageName, Dictionary<string, string> environmentVariables);
        Task RemoveAsync(string id);
    }
}
