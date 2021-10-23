using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface ITaskRepositoryFactory
    {
        ITaskRepository Create();
    }
}