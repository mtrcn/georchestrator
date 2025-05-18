using Amazon.ECS;
using Amazon.ECS.Model;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Container = GEOrchestrator.Domain.Models.Containers.Container;
using KeyValuePair = Amazon.ECS.Model.KeyValuePair;
using Task = System.Threading.Tasks.Task;

namespace GEOrchestrator.Business.Providers.ContainerProviders.Fargate
{
    public class FargateContainerProvider : IContainerRepository
    {
        private readonly IAmazonECS _amazonEcsClient;
        private readonly string _awsRegion;
        private readonly string _executionRolArn;
        private readonly string _clusterName;
        private readonly string _subnetId;
        private readonly string _securityGroupId;

        public FargateContainerProvider(IAmazonECS amazonEcsClient, IConfiguration configuration)
        {
            _amazonEcsClient = amazonEcsClient;
            _awsRegion = configuration["FARGATE_REGION"];
            _executionRolArn = configuration["FARGATE_EXECUTION_ROLE_ARN"];
            _clusterName = configuration["FARGATE_CLUSTER_NAME"];
            _subnetId = configuration["FARGATE_SUBNET_ID"];
            _securityGroupId = configuration["FARGATE_SECURITY_GROUP_ID"];
        }

        public async Task<Container> RunAsync(string imageName, Dictionary<string, string> environmentVariables)
        {

            var id = Guid.NewGuid().ToString();

            var registerTaskDefinitionRequest = new RegisterTaskDefinitionRequest {
                Family = id,
                Cpu = "256",
                Memory = "2048",
                RequiresCompatibilities = new List<string> { "FARGATE" },
                NetworkMode = NetworkMode.Awsvpc,
                ExecutionRoleArn = _executionRolArn,
                ContainerDefinitions = new List<ContainerDefinition>
                {
                    new ContainerDefinition
                    {
                        Name = id,
                        Image = imageName,
                        Environment = environmentVariables.Select(k => new KeyValuePair { Name = k.Key, Value = k.Value }).ToList(),
                        LogConfiguration = new LogConfiguration
                        {
                            LogDriver = new LogDriver("awslogs"),
                            Options = new Dictionary<string, string>
                            {
                                { "awslogs-group", "georchestrator" },
                                { "awslogs-region", _awsRegion },
                                { "awslogs-stream-prefix", id }
                            }
                        }
                    }
                }
            };
            var registerTaskDefinitionResponse = await _amazonEcsClient.RegisterTaskDefinitionAsync(registerTaskDefinitionRequest);

            await _amazonEcsClient.RunTaskAsync(new RunTaskRequest
            {
                Cluster = _clusterName,
                TaskDefinition = registerTaskDefinitionResponse.TaskDefinition.TaskDefinitionArn,
                LaunchType = LaunchType.FARGATE,
                NetworkConfiguration = new NetworkConfiguration
                {
                    AwsvpcConfiguration = new AwsVpcConfiguration
                    {
                        AssignPublicIp = new AssignPublicIp("ENABLED"),
                        Subnets = new List<string> { _subnetId },
                        SecurityGroups = new List<string> { _securityGroupId },
                    }
                }
            });

            return new Container
            {
                Id = id
            };
        }

        public async Task RemoveAsync(string id)
        {
            await _amazonEcsClient.DeregisterTaskDefinitionAsync(new DeregisterTaskDefinitionRequest
            {
                TaskDefinition = $"{id}:1"
            });
        }
    }
}
