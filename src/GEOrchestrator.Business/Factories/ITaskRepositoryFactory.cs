using GEOrchestrator.Business.Repositories.Tasks;

namespace GEOrchestrator.Business.Factories
{
    public interface ITaskRepositoryFactory
    {
        ITaskRepository Create();
    }
}