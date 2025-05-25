using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Providers.DatabaseProviders.Redis;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ParameterRepositoryFactory : IParameterRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public ParameterRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["DATABASE_REPOSITORY_PROVIDER"];
        }

        public IParameterRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbParameterRepository>(),
                "redis" => _serviceProvider.GetService<RedisParameterRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known database repository provider")
            };
        }
    }
}
