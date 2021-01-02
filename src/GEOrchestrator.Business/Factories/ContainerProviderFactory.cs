using System;
using GEOrchestrator.Business.Providers.ContainerProviders;
using GEOrchestrator.Business.Providers.ContainerProviders.Docker;
using GEOrchestrator.Business.Providers.ContainerProviders.Fargate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Business.Factories
{
    public class ContainerProviderFactory : IContainerProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _provider;

        public ContainerProviderFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _provider = configuration["CONTAINER_PROVIDER"];
        }

        public IContainerProvider Create()
        {
            return _provider switch
            {
                "docker" => _serviceProvider.GetService<DockerContainerProvider>(),
                "fargate" => _serviceProvider.GetService<FargateContainerProvider>(),
                _ => throw new InvalidOperationException($"{_provider} is not known container provider")
            };
        }
    }
}
