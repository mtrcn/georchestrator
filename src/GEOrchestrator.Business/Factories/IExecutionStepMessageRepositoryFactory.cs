using GEOrchestrator.Business.Repositories.Executions;

namespace GEOrchestrator.Business.Factories
{
    public interface IExecutionStepMessageRepositoryFactory
    {
        IExecutionStepMessageRepository Create();
    }
}