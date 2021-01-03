using GEOrchestrator.Domain.Models.Workflows;
using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Tasks
{
    public class Iterate
    {
        public string Collection { get; set; }
        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public int MaxConcurrency { get; set; } = 1;
    }
}
