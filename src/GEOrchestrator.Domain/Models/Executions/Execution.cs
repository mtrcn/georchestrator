using System;
using System.Collections.Generic;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class Execution
    {
        public string Id { get; set; }
        public string ParentExecutionId { get; set; }
        public string ParentStepId { get; set; }
        public string WorkflowRunId { get; set; }
        public string WorkflowName { get; set; }
        public int WorkflowVersion { get; set; }
        public string Status { get; set; }
        public ExecutionIteration Iteration { get; set; }
        public List<WorkflowStep> Steps { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
    }
}
