using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IExecutionStepRepositoryFactory
    {
        IStepExecutionRepository Create();
    }
}