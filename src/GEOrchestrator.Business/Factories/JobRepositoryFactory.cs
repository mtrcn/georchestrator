using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Providers.DatabaseProviders.Redis;
using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Factories
{
    public class JobRepositoryFactory : IJobRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public JobRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["DATABASE_REPOSITORY_PROVIDER"];
        }

        public IJobRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbJobRepository>(),
                "redis" => _serviceProvider.GetService<RedisJobRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known database repository provider")
            };
        }
    }
}
