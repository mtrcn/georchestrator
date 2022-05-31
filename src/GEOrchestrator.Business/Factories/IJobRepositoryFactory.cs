using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IJobRepositoryFactory
    {
        IJobRepository Create();
    }
}