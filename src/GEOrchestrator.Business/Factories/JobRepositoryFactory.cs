﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
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
            _repositoryProvider = configuration["JOB_REPOSITORY_PROVIDER"];
        }

        public IJobRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbJobRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known job repository provider")
            };
        }
    }
}
