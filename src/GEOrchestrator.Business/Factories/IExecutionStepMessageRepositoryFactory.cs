using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IExecutionStepMessageRepositoryFactory
    {
        IStepExecutionMessageRepository Create();
    }
}