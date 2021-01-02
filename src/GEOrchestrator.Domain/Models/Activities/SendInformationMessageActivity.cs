using System;

namespace GEOrchestrator.Domain.Models.Activities
{
    public class SendInformationMessageActivity
    {
        public string Message { get; set; }
        public DateTime SentOn { get; set; }
    }
}
