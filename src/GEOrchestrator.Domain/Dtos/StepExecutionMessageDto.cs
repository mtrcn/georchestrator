using System;

namespace GEOrchestrator.Domain.Dtos
{
    public class StepExecutionMessageDto
    {
        public string StepExecutionId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
