namespace GEOrchestrator.Domain.Models.Executions
{
    public class StepExecution
    {
        public string Id { get; set; }
        public string JobId { get; set; }
        public string ParentStepExecutionId { get; set; }
        public string StepId { get; set; }
        public string Task { get; set; }
        public int Iteration { get; set; }
        public int CompletedIteration { get; set; }
        public int TotalIteration { get; set; }
        public string Status { get; set; }
        public string ContainerId { get; set; }
        public string ContainerDetails { get; set; }
    }
}
