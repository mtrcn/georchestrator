using System.Collections.Generic;

namespace GEOrchestrator.Domain.Dtos
{
    public class ExecuteDto
    {
        public Dictionary<string, string> Inputs { get; set; }
        public string Response { get; set; } = "raw";
    }
}
