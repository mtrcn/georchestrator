using System;

namespace GEOrchestrator.Business.Exceptions
{
    public class WorkflowValidationException: Exception
    {
        public WorkflowValidationException(string message) : base(message)
        {
            
        }
    }
}
