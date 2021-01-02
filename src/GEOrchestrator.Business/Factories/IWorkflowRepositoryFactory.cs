using GEOrchestrator.Business.Repositories.Workflow;

namespace GEOrchestrator.Business.Factories
{
    public interface IWorkflowRepositoryFactory
    {
        IWorkflowRepository Create();
    }
}