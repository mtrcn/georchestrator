namespace GEOrchestrator.Domain.Models.Artifacts
{
    public class NextExecutionArtifactRequest
    {
        public string WorkflowRunId { get; set; }
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string Marker { get; set; }
    }
}
