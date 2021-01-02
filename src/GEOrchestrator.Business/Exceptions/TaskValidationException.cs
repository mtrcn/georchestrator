using System;

namespace GEOrchestrator.Business.Exceptions
{
    public class TaskValidationException: Exception
    {
        public TaskValidationException(string message) : base(message)
        {
            
        }
    }
}
