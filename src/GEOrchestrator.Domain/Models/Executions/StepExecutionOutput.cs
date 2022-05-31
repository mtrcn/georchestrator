using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class StepExecutionOutput
    {
        public Dictionary<string, string> Artifacts { get; set; } = new Dictionary<string, string>();
    }
}
