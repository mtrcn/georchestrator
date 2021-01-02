using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using MediatR;
using SharpYaml.Serialization;

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
            var serializer = new Serializer();
            var task = serializer.DeserializeInto(notification.TaskDefinition, new Domain.Models.Tasks.Task());
            await _taskService.SaveAsync(task);
        }
    }
}
