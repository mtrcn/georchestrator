using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SaveTaskEvent : INotification
    {
        public SaveTaskEvent(string taskDefinition)
        {
            TaskDefinition = taskDefinition;
        }

        public string TaskDefinition { get; }
    }
}
