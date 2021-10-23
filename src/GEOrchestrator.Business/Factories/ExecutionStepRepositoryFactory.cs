using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Repositories;
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

        public IStepExecutionRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbStepExecutionRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known execution step repository provider")
            };
        }
    }
}
