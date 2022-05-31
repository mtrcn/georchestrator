using Pulumi;

namespace GEOrchestrator.Aws.Deployment
{
    public class TableOutputs
    {
        public Output<string> JobsTableArn { get; set; }
        public Output<string> ParametersTableArn { get; set; }
        public Output<string> ArtifactsTableArn { get; set; }
        public Output<string> WorkflowsTableArn { get; set; }
        public Output<string> TasksTableArn { get; set; }
        public Output<string> ExecutionStepsTableArn { get; set; }
        public Output<string> ExecutionStepMessagesTableArn { get; set; }
    }
}
