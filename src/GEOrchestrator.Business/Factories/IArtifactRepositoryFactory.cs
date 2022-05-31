using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IArtifactRepositoryFactory
    {
        IArtifactRepository Create();
    }
}