using Amazon.DynamoDBv2;
using Amazon.ECS;
using Amazon.S3;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Providers.ContainerProviders.Docker;
using GEOrchestrator.Business.Providers.ContainerProviders.Fargate;
using GEOrchestrator.Business.Repositories.Artifacts;
using GEOrchestrator.Business.Repositories.Executions;
using GEOrchestrator.Business.Repositories.Objects;
using GEOrchestrator.Business.Repositories.Parameters;
using GEOrchestrator.Business.Repositories.Tasks;
using GEOrchestrator.Business.Repositories.Workflow;
using GEOrchestrator.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.Api.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IWorkflowService, WorkflowService>();
            services.AddTransient<IParameterService, ParameterService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<IWorkflowValidatorService, WorkflowValidatorService>();
            services.AddTransient<ITaskValidatorService, TaskValidatorService>();
            services.AddTransient<IArtifactService, ArtifactService>();
            services.AddTransient<IExecutionService, ExecutionService>();
            return services;
        }
        
        public static IServiceCollection AddAwsServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonECS>();
            return services;
        }

        public static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddTransient<IObjectRepositoryFactory, ObjectRepositoryFactory>();
            services.AddTransient<IArtifactRepositoryFactory, ArtifactRepositoryFactory>();
            services.AddTransient<IParameterRepositoryFactory, ParameterRepositoryFactory>();
            services.AddTransient<IExecutionStepRepositoryFactory, ExecutionStepRepositoryFactory>();
            services.AddTransient<IExecutionStepMessageRepositoryFactory, ExecutionStepMessageRepositoryFactory>();
            services.AddTransient<IExecutionRepositoryFactory, ExecutionRepositoryFactory>();
            services.AddTransient<IWorkflowRepositoryFactory, WorkflowRepositoryFactory>();
            services.AddTransient<ITaskRepositoryFactory, TaskRepositoryFactory>();
            services.AddTransient<IContainerProviderFactory, ContainerProviderFactory>();
            return services;
        }

        public static IServiceCollection AddAwsRepositories(this IServiceCollection services)
        {
            services.AddTransient<AmazonS3ObjectRepository>();
            services.AddTransient<DynamoDbArtifactRepository>();
            services.AddTransient<DynamoDbParameterRepository>();
            services.AddTransient<DynamoDbExecutionStepRepository>();
            services.AddTransient<DynamoDbExecutionStepMessageRepository>();
            services.AddTransient<DynamoDbExecutionRepository>();
            services.AddTransient<DynamoDbWorkflowRepository>();
            services.AddTransient<DynamoDbTaskRepository>();
            return services;
        } 

        public static IServiceCollection AddLocalProviders(this IServiceCollection services)
        {
            services.AddTransient<DockerContainerProvider>();
            services.AddTransient<FargateContainerProvider>();
            return services;
        } 
    }
}
