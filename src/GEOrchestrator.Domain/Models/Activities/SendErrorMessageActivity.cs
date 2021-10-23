using System;

namespace GEOrchestrator.Domain.Models.Activities
{
    public class ErrorMessageActivity
    {
        public string Message { get; set; }
        public DateTime SentOn { get; set; }
    }
}
