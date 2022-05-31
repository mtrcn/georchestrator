using Pulumi;

namespace GEOrchestrator.Aws.Deployment
{
    public class WorkflowManagerArguments
    {
        public Output<string> S3BucketName { get; set; }
        public Output<string> S3BucketArn { get; set; }
        public Output<string> JobsTableArn { get; set; }
        public Output<string> ParametersTableArn { get; set; }
        public Output<string> ArtifactsTableArn { get; set; }
        public Output<string> WorkflowsTableArn { get; set; }
        public Output<string> TasksTableArn { get; set; }
        public Output<string> ExecutionStepsTableArn { get; set; }
        public Output<string> ExecutionStepMessagesTableArn { get; set; }
        public string ApiUrl { get; set; }
        public Output<string> FargateRegion { get; set; }
        public Output<string> FargateClusterArn { get; set; }
        public Output<string> FargateClusterName { get; set; }
        public Output<string> FargateSubnetId { get; set; }
        public Output<string> FargateSecurityGroupId { get; set; }
    }
}
