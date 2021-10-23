using System;

namespace GEOrchestrator.Domain.Dtos
{
    public class JobStatusDto
    {
        public string JobId { get; set; }
        public string ProcessId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime? Started { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime Updated { get; set; }
    }
}
