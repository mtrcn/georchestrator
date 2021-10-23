using Amazon.DynamoDBv2;
using Amazon.ECS;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.WorkflowManager.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddAwsServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonECS>();
        }
    }
}
