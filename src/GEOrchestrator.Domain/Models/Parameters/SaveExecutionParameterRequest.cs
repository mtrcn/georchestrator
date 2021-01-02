namespace GEOrchestrator.Domain.Models.Parameters
{
    public class SaveExecutionParameterRequest
    {
        public string WorkflowRunId { get; set; }
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
