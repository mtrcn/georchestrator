using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Workflows
{
    public class WorkflowInput
    {
        public List<WorkflowInputParameter> Parameters { get; set; } = new List<WorkflowInputParameter>();
    }
}
