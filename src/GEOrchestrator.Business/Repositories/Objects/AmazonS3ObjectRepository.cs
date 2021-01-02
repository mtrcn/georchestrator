using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace GEOrchestrator.Business.Repositories.Objects
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

        public async Task AddAsync(string path, byte[] content)
        {
            var putObjectRequest = new PutObjectRequest
            {
                InputStream = new MemoryStream(content),
                AutoCloseStream = true,
                Key = $"{_prefix}{path}",
                BucketName = _bucketName
            };

            await _amazonS3.PutObjectAsync(putObjectRequest);
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

        public async Task<byte[]> GetAsync(string path)
        {
            var getObjectRequest = new GetObjectRequest
            {
                Key = $"{_prefix}{path}",
                BucketName = _bucketName
            };

            await using var stream = new MemoryStream();
            using var response = await _amazonS3.GetObjectAsync(getObjectRequest);
            await using var responseStream = response.ResponseStream;
            responseStream.CopyTo(stream);
            return stream.ToArray();
        }
    }
}
