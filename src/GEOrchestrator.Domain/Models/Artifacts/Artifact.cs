namespace GEOrchestrator.Domain.Models.Artifacts
{
    public class Artifact
    {
        public string JobId { get; set; }
        public string StepId { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string StoragePath { get; set; }
    }
}
