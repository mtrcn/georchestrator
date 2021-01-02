using GEOrchestrator.Business.Repositories.Parameters;

namespace GEOrchestrator.Business.Factories
{
    public interface IParameterRepositoryFactory
    {
        IParameterRepository Create();
    }
}