using System.Collections.Generic;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Domain.Models.Executions
{
    public class ExecutionNode
    {
        public WorkflowStep Node { get; set; }
        public WorkflowStep Parent { get; set; }
        public List<ExecutionNode> Children { get; set; } = new List<ExecutionNode>();
        public WorkflowStep PreviousStep { get; set; }
        public WorkflowStep NextStep { get; set; }
    }
}
