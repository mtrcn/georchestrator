using GEOrchestrator.Business.Providers.ContainerProviders;
using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IContainerProviderFactory
    {
        IContainerRepository Create();
    }
}