using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public interface IParameterRepositoryFactory
    {
        IParameterRepository Create();
    }
}