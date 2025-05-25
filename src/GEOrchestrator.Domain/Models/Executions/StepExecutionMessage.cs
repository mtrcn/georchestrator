using System;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class StepExecutionMessage
    {
        public string JobId { get; set; }
        public string StepExecutionId { get; set; }
        public string Type { get; set; }
        public DateTime SentOn { get; set; }
        public string Message { get; set; }
    }
}
