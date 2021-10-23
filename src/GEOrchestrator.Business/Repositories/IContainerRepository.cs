using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Containers;

namespace GEOrchestrator.Business.Repositories
{
    public interface IContainerRepository
    {
        Task<Container> RunAsync(string imageName, Dictionary<string, string> environmentVariables);
        Task RemoveAsync(string id);
    }
}
