using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

namespace GEOrchestrator.TaskManager.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddAwsServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
        }
    }
}
