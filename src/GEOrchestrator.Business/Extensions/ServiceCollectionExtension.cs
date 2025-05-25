using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Providers.ContainerProviders.Docker;
using GEOrchestrator.Business.Providers.ContainerProviders.Fargate;
using GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb;
using GEOrchestrator.Business.Providers.DatabaseProviders.Redis;
using GEOrchestrator.Business.Providers.ObjectStorageProviders.AmazonS3;
using GEOrchestrator.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

        public static void AddAwsRepositories(this IServiceCollection services)
        {
            services.AddTransient<AmazonS3ObjectRepository>();
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
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(redis);

            services.AddTransient<RedisJobRepository>();
            services.AddTransient<RedisTaskRepository>();
            services.AddTransient<RedisWorkflowRepository>();
            services.AddTransient<RedisStepExecutionRepository>();
            services.AddTransient<RedisStepExecutionMessageRepository>();
            services.AddTransient<RedisArtifactRepository>();
            services.AddTransient<RedisParameterRepository>();
        }

        public static void AddContainerProviders(this IServiceCollection services)
        {
            services.AddTransient<DockerContainerProvider>();
            services.AddTransient<FargateContainerProvider>();
        }
    }
}
