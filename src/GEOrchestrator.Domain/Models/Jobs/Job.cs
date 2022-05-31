using GEOrchestrator.Domain.Models.Workflows;
using System;

namespace GEOrchestrator.Domain.Models.Jobs
{
    public class Job
    {
        public string Id { get; set; }
        public string WorkflowName { get; set; }
        public int WorkflowVersion { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public Workflow Workflow { get; set; }
        public DateTime? Started { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime Updated { get; set; }
    }
}
