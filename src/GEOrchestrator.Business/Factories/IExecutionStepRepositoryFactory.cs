using GEOrchestrator.Business.Repositories.Executions;

namespace GEOrchestrator.Business.Factories
{
    public interface IExecutionStepRepositoryFactory
    {
        IExecutionStepRepository Create();
    }
}