using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Parameters;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public class ParameterService : IParameterService
    {
        private readonly IParameterRepository _parameterRepository;

        public ParameterService(
            IExecutionStepRepositoryFactory executionStepRepositoryFactory, 
            IParameterRepositoryFactory parameterRepositoryFactory)
        {
            _parameterRepository = parameterRepositoryFactory.Create();
        }

        public async Task SaveParameterAsync(Parameter parameter)
        {
            await _parameterRepository.AddAsync(parameter);
        }
    }
}
