using GEOrchestrator.Business.Providers.ContainerProviders;

namespace GEOrchestrator.Business.Factories
{
    public interface IContainerProviderFactory
    {
        IContainerProvider Create();
    }
}