using Amazon.DynamoDBv2;
using Amazon.ECS;
using Amazon.S3;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Providers.ContainerProviders.Docker;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Providers.DatabaseProviders.Redis;
using GEOrchestrator.Business.Providers.ObjectStorageProviders.AmazonS3;
using GEOrchestrator.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace GEOrchestrator.Business.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IContainerService, ContainerService>();
            services.AddTransient<IWorkflowService, WorkflowService>();
            services.AddTransient<IParameterService, ParameterService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<IWorkflowValidatorService, WorkflowValidatorService>();
            services.AddTransient<ITaskValidatorService, TaskValidatorService>();
            services.AddTransient<IArtifactService, ArtifactService>();
            services.AddTransient<IStepExecutionService, StepExecutionService>();
        }

        public static void AddFactories(this IServiceCollection services)
        {
            services.AddTransient<IObjectRepositoryFactory, ObjectRepositoryFactory>();
            services.AddTransient<IArtifactRepositoryFactory, ArtifactRepositoryFactory>();
            services.AddTransient<IParameterRepositoryFactory, ParameterRepositoryFactory>();
            services.AddTransient<IExecutionStepRepositoryFactory, ExecutionStepRepositoryFactory>();
            services.AddTransient<IExecutionStepMessageRepositoryFactory, ExecutionStepMessageRepositoryFactory>();
            services.AddTransient<IWorkflowRepositoryFactory, WorkflowRepositoryFactory>();
            services.AddTransient<ITaskRepositoryFactory, TaskRepositoryFactory>();
            services.AddTransient<IJobRepositoryFactory, JobRepositoryFactory>();
            services.AddTransient<IContainerProviderFactory, ContainerProviderFactory>();
        }

        public static void AddObjectStorageProviders(this IServiceCollection services, IConfiguration configuration)
        {
            var objectStorageProvider = configuration["OBJECT_REPOSITORY_PROVIDER"];
            switch (objectStorageProvider)
            {
                case "s3":
                    services.AddAmazonS3ObjectStorage();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported object repository provider: {objectStorageProvider}");
            }
        }

        public static void AddAmazonS3ObjectStorage(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var endpointUrl = configuration["AWS_ENDPOINT_URL_S3"];
                if (string.IsNullOrEmpty(endpointUrl))
                {
                    return new AmazonS3Client();
                }

                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS_REGION"]),
                    ForcePathStyle = true,
                    ServiceURL = endpointUrl,
                    UseHttp = endpointUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                };

                return new AmazonS3Client(config);
            });

            services.AddTransient<AmazonS3ObjectRepository>();
        }

        public static void AddDatabaseRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseProvider = configuration["DATABASE_REPOSITORY_PROVIDER"];

            switch (databaseProvider)
            {
                case "redis":
                    services.AddRedisRepositories(configuration);
                    break;
                case "dynamodb":
                    services.AddDynamodbRepositories();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported database repository provider: {databaseProvider}");
            }
        }

        public static void AddDynamodbRepositories(this IServiceCollection services)
        {
            services.AddTransient<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddTransient<DynamoDbJobRepository>();
            services.AddTransient<DynamoDbArtifactRepository>();
            services.AddTransient<DynamoDbParameterRepository>();
            services.AddTransient<DynamoDbStepExecutionRepository>();
            services.AddTransient<DynamoDbStepExecutionMessageRepository>();
            services.AddTransient<DynamoDbWorkflowRepository>();
            services.AddTransient<DynamoDbTaskRepository>();
        }

        public static void AddRedisRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["REDIS_CONNECTION_STRING"];
            var redis = ConnectionMultiplexer.Connect(redisConnectionString ?? throw new InvalidOperationException("Redis connection string is not provided."));
            services.AddSingleton<IConnectionMultiplexer>(redis);

            services.AddTransient<RedisJobRepository>();
            services.AddTransient<RedisTaskRepository>();
            services.AddTransient<RedisWorkflowRepository>();
            services.AddTransient<RedisStepExecutionRepository>();
            services.AddTransient<RedisStepExecutionMessageRepository>();
            services.AddTransient<RedisArtifactRepository>();
            services.AddTransient<RedisParameterRepository>();
        }

        public static void AddContainerProviders(this IServiceCollection services, IConfiguration configuration)
        {
            var containerProvider = configuration["CONTAINER_PROVIDER"];

            switch (containerProvider)
            {
                case "docker":
                    services.AddTransient<DockerContainerProvider>();
                    break;
                case "fargate":
                    services.AddFargateContainerProvider();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported container provider: {containerProvider}");
            }
        }

        public static void AddFargateContainerProvider(this IServiceCollection services)
        {
            services.AddTransient<IAmazonECS, AmazonECSClient>();
        }
    }
}
