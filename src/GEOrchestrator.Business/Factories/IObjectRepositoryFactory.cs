using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IObjectRepositoryFactory
    {
        IObjectRepository Create();
    }
}