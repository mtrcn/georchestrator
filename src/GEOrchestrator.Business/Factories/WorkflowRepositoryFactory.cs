using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Providers.DatabaseProviders.Redis;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class WorkflowRepositoryFactory : IWorkflowRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public WorkflowRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["DATABASE_REPOSITORY_PROVIDER"];
        }

        public IWorkflowRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbWorkflowRepository>(),
                "redis" => _serviceProvider.GetService<RedisWorkflowRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known database repository provider")
            };
        }
    }
}
