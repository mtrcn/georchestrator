using System;
using GEOrchestrator.Business.Providers.ObjectStorageProviders.AmazonS3;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ObjectRepositoryFactory : IObjectRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _repositoryProvider;

        public ObjectRepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _repositoryProvider = configuration["OBJECT_REPOSITORY_PROVIDER"];
        }

        public IObjectRepository Create()
        {
            return _repositoryProvider switch
            {
                "s3" => _serviceProvider.GetService<AmazonS3ObjectRepository>(),
                _ => throw new InvalidOperationException($"{_repositoryProvider} is not known object repository provider")
            };
        }
    }
}
