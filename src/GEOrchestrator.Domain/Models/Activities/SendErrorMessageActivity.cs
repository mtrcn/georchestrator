using System;

namespace GEOrchestrator.Domain.Models.Activities
{
    public class SendErrorMessageActivity
    {
        public string Message { get; set; }
        public DateTime SentOn { get; set; }
    }
}
