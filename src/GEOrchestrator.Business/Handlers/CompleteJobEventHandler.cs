using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class CompleteJobEventHandler : INotificationHandler<CompleteJobEvent>
    {
        private readonly IJobService _jobService;

        public CompleteJobEventHandler(IJobService jobService)
        {
            _jobService = jobService;
        }

        public async Task Handle(CompleteJobEvent notification, CancellationToken cancellationToken)
        {
            await _jobService.SaveJobStatusAsync(notification.JobId, notification.Status, notification.Message);
            await _jobService.FinishJobAsync(notification.JobId);
        }
    }
}
