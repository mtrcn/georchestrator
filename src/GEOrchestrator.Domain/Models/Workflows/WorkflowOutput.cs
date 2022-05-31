using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Workflows
{
    public class WorkflowOutput
    {
        public List<WorkflowOutputArtifact> Artifacts { get; set; } = new List<WorkflowOutputArtifact>();
        public List<WorkflowOutputParameter> Parameters { get; set; } = new List<WorkflowOutputParameter>();
    }
}
