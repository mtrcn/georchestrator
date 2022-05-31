using Pulumi;

namespace GEOrchestrator.Aws.Deployment
{
    public class ObjectRepositoryOutputs
    {
        public Output<string> S3BucketName { get; set; }
        public Output<string> S3BucketArn { get; set; }
    }
}
