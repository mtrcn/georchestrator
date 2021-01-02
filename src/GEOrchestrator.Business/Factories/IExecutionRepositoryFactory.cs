using GEOrchestrator.Business.Repositories.Executions;

namespace GEOrchestrator.Business.Factories
{
    public interface IExecutionRepositoryFactory
    {
        IExecutionRepository Create();
    }
}