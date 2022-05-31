namespace GEOrchestrator.Domain.Models.Artifacts
{
    public class NextExecutionArtifactResponse
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string Url { get; set; }
        public string Marker { get; set; }
    }
}
