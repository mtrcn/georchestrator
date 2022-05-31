using System.Collections.Generic;

namespace GEOrchestrator.Domain.Models.Tasks
{
    public class Task
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public List<TaskInput> Inputs { get; set; } = new List<TaskInput>();
        public List<TaskOutput> Outputs { get; set; } = new List<TaskOutput>();
    }
}
