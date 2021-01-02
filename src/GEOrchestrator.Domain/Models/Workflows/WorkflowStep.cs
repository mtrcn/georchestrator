using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Workflows
{
    public class WorkflowStep
    {
        public string Id { get; set; }
        public string Task { get; set; }
        public string Description { get; set; }
        public List<WorkflowStep> Iterate { get; set; } = new List<WorkflowStep>();
        public List<List<WorkflowStep>> Branches { get; set; } = new List<List<WorkflowStep>>();
        public WorkflowStepInput Inputs { get; set; }
        public WorkflowStepOutput Outputs { get; set; }
    }
}
