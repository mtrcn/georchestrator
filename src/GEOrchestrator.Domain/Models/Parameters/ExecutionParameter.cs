namespace GEOrchestrator.Domain.Models.Parameters
{
    public class ExecutionParameter
    {
        public string JobId { get; set; }
        public string ExecutionId { get; set; }
        public string SourceType { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
