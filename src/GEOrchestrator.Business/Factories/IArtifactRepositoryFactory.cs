using GEOrchestrator.Business.Repositories.Artifacts;

namespace GEOrchestrator.Business.Factories
{
    public interface IArtifactRepositoryFactory
    {
        IArtifactRepository Create();
    }
}