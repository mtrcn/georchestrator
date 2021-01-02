using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Workflows
{
    public class Workflow
    {
        public string Name { get; set; }

        public int Version { get; set; }

        public string Description { get; set; }

        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}
