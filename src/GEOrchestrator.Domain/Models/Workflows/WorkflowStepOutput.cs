using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Workflows
{
    public class WorkflowStepOutput
    {
        public List<WorkflowStepOutputArtifact> Artifacts { get; set; } = new List<WorkflowStepOutputArtifact>();
        public List<WorkflowStepOutputParameter> Parameters { get; set; } = new List<WorkflowStepOutputParameter>();
    }
}
