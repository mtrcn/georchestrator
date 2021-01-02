using System;
using GEOrchestrator.Business.Repositories.Executions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ExecutionRepositoryFactory : IExecutionRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public ExecutionRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["EXECUTION_REPOSITORY_PROVIDER"];
        }

        public IExecutionRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbExecutionRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known execution repository provider")
            };
        }
    }
}
