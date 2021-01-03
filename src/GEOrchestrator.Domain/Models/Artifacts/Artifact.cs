namespace GEOrchestrator.Domain.Models.Artifacts
{
    public class Artifact
    {
        public string WorkflowRunId { get; set; }
        public string StepId { get; set; }
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string StoragePath { get; set; }
    }
}
