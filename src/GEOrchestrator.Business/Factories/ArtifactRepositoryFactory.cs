﻿using System;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ArtifactRepositoryFactory : IArtifactRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public ArtifactRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["ARTIFACT_REPOSITORY_PROVIDER"];
        }

        public IArtifactRepository Create()
        {
            return _repositoryProvider switch
            {
                "dynamodb" => _serviceProvider.GetService<DynamoDbArtifactRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known artifact repository provider")
            };
        }
    }
}
