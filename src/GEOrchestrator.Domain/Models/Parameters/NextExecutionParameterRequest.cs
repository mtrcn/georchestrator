namespace GEOrchestrator.Domain.Models.Parameters
{
    public class NextExecutionParameterRequest
    {
        public string WorkflowRunId { get; set; }
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string Marker { get; set; }
    }
}
