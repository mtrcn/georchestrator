using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class SaveTaskEventHandler : INotificationHandler<SaveTaskEvent>
    {
        private readonly ITaskService _taskService;

        public SaveTaskEventHandler(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public async Task Handle(SaveTaskEvent notification, CancellationToken cancellationToken)
        {
            await _taskService.SaveAsync(notification.Task);
        }
    }
}
