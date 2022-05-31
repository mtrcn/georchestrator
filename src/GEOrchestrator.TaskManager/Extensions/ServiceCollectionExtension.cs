using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.TaskManager.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddAwsServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonDynamoDB>();
        }
    }
}
