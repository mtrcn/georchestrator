using Pulumi;

namespace GEOrchestrator.Aws.Deployment
{
    public class ContainerOrchestratorOutputs
    {
        public Output<string> ClusterName { get; set; }
        public Output<string> ClusterArn { get; set; }
    }
}
