using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GEOrchestrator.Domain.Models.Executions
{
    [JsonSerializable(typeof(StepExecutionInput))]
    public class StepExecutionInput
    {
        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        
        [JsonPropertyName("artifacts")]
        public Dictionary<string, string> Artifacts { get; set; } = new Dictionary<string, string>();
    }
}
