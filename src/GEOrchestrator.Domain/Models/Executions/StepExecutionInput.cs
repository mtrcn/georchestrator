using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class StepExecutionInput
    {
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Artifacts { get; set; } = new Dictionary<string, string>();
    }
}
