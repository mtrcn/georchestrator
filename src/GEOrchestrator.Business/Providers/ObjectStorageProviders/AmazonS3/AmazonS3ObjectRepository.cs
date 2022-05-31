using Amazon.S3;
using Amazon.S3.Model;
using GEOrchestrator.Business.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Providers.ObjectStorageProviders.AmazonS3
{
    public class AmazonS3ObjectRepository : IObjectRepository
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly string _bucketName;
        private readonly string _prefix;

        public AmazonS3ObjectRepository(IAmazonS3 amazonS3, IConfiguration configuration)
        {
            _amazonS3 = amazonS3;
            _bucketName = configuration["AWS_S3_BUCKET_NAME"];
            _prefix = configuration["AWS_S3_PREFIX"];
        }

        public Task<string> GetSignedUrlForDownloadAsync(string path)
        {
            var getPreSignedUrlRequest = new GetPreSignedUrlRequest
            {
                Key = $"{_prefix}{path}",
                Expires = DateTime.UtcNow.AddHours(1),
                BucketName = _bucketName,
                Verb = HttpVerb.GET
            };

            var response = _amazonS3.GetPreSignedURL(getPreSignedUrlRequest);
            return Task.FromResult(response);
        }

        public Task<string> GetSignedUrlForUploadAsync(string path)
        {
            var getPreSignedUrlRequest = new GetPreSignedUrlRequest
            {
                Key = $"{_prefix}{path}",
                Expires = DateTime.UtcNow.AddHours(1),
                BucketName = _bucketName,
                Verb = HttpVerb.PUT
            };

            var response = _amazonS3.GetPreSignedURL(getPreSignedUrlRequest);
            return Task.FromResult(response);
        }
    }
}
