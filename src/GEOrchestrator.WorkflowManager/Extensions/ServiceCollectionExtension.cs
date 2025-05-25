using Amazon.DynamoDBv2;
using Amazon.ECS;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime;
using System;

namespace GEOrchestrator.WorkflowManager.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddAwsServices(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var credentials = new BasicAWSCredentials(
                    configuration["AWS_ACCESS_KEY_ID"],
                    configuration["AWS_SECRET_ACCESS_KEY"]
                );
                
                var endpointUrl = configuration["AWS_ENDPOINT_URL_S3"];
                if (string.IsNullOrEmpty(endpointUrl))
                {
                    return new AmazonS3Client(credentials);
                }

                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS_REGION"]),
                    ForcePathStyle = true,
                    ServiceURL = endpointUrl,
                    UseHttp = endpointUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                };

                return new AmazonS3Client(credentials, config);
            });
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonECS>();
        }
    }
}
