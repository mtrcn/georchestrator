using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ExecutionStepMessageRepositoryFactory : IExecutionStepMessageRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public ExecutionStepMessageRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["EXECUTION_STEP_MESSAGE_REPOSITORY_PROVIDER"];
        }

        public IStepExecutionMessageRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbStepExecutionMessageRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known execution step message repository provider")
            };
        }
    }
}
