using System.Collections.Generic;

namespace GEOrchestrator.Domain.Dtos
{
    public class ProcessDto
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public List<string> JobControlOptions { get; set; } = new List<string> {"async-execute"};
        public List<string> OutputTransmission { get; set; } = new List<string> {"value", "reference"};
    }
}
