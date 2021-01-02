using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Workflows
{
    public class WorkflowStepInput
    {
        public List<WorkflowStepInputArtifact> Artifacts { get; set; } = new List<WorkflowStepInputArtifact>();
        public List<WorkflowStepInputParameter> Parameters { get; set; } = new List<WorkflowStepInputParameter>();
    }
}
