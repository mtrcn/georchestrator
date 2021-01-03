namespace GEOrchestrator.Domain.Models.Executions
{
    public class ExecutionIteration
    {
        public int Index { get; set; }
        public int MaxConcurrency { get; set; } = 1;
        public string ItemValue { get; set; }
        public string ItemType { get; set; }
    }
}
