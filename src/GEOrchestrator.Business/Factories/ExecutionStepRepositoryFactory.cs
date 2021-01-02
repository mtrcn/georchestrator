using System;
using GEOrchestrator.Business.Repositories.Executions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ExecutionStepRepositoryFactory : IExecutionStepRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public ExecutionStepRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["EXECUTION_STEP_REPOSITORY_PROVIDER"];
        }

        public IExecutionStepRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbExecutionStepRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known execution step repository provider")
            };
        }
    }
}
