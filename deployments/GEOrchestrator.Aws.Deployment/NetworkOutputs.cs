using Pulumi;

namespace GEOrchestrator.Aws.Deployment
{
    public class NetworkOutputs
    {
        public Output<string> FargateSubnetId { get; set; }
        public Output<string> SecurityGroupId { get; set; }
    }
}
