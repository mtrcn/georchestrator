namespace GEOrchestrator.Domain.Models.Executions
{
    public class ExecutionIteration
    {
        public int Index { get; set; }
        public string CollectionValue { get; set; }
        public string CollectionType { get; set; }
        public string Marker { get; set; }
    }
}
