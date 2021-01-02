using System;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class ExecutionStepMessage
    {
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string Type { get; set; }
        public DateTime SentOn { get; set; }
        public string Message { get; set; }
    }
}
