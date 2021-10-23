using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IWorkflowRepositoryFactory
    {
        IWorkflowRepository Create();
    }
}