using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class ExecutionStep
    {
        public string WorkflowRunId { get; set; }
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string Status { get; set; }
        public WorkflowStep Step { get; set; }
        public string ContainerId { get; set; }
        public string ContainerDetails { get; set; }
    }
}
