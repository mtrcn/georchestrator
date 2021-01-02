namespace GEOrchestrator.Domain.Models.Executions
{
    public class ExecutionStepActivity
    {
        public string ActivityPayload { get; set; }
        public string ActivityType { get; set; }
        public string StepId { get; set; }
        public string ExecutionId { get; set; }
    }
}
