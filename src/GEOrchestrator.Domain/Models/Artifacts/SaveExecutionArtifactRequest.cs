namespace GEOrchestrator.Domain.Models.Artifacts
{
    public class SaveExecutionArtifactRequest
    {
        public string WorkflowRunId { get; set; }
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string RelativePath { get; set; }
    }
}
