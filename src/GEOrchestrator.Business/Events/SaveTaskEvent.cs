using GEOrchestrator.Domain.Models.Tasks;
using MediatR;

namespace GEOrchestrator.Business.Events
{
    public class SaveTaskEvent : INotification
    {
        public SaveTaskEvent(Task task)
        {
            Task = task;
        }

        public Task Task { get; }
    }
}
