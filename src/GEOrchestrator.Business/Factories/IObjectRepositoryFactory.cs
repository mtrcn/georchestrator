using GEOrchestrator.Business.Repositories.Objects;

namespace GEOrchestrator.Business.Factories
{
    public interface IObjectRepositoryFactory
    {
        IObjectRepository Create();
    }
}