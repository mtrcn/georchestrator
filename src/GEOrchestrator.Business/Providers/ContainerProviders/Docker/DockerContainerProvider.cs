using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Containers;

namespace GEOrchestrator.Business.Providers.ContainerProviders.Docker
{
    public class DockerContainerProvider : IContainerRepository
    {
        private readonly IDockerClient _dockerClient;
        public DockerContainerProvider()
        {
            _dockerClient = new DockerClientConfiguration().CreateClient();
        }

        public async Task<Container> RunAsync(string imageName, Dictionary<string, string> environmentVariables)
        {
            var response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters(new Config
            {
                Image = imageName,
                Env = environmentVariables.Select(e => $"{e.Key}={e.Value}").ToList()
            }));

            await _dockerClient.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());

            return new Container
            {
                Id = response.ID
            };
        }

        public async Task RemoveAsync(string id)
        {
            await _dockerClient.Containers.StopContainerAsync(id, new ContainerStopParameters());
            await _dockerClient.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters());
        }
    }
}
