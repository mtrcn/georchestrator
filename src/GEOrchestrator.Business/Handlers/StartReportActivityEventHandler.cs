using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Handlers
{
    public class StartReportActivityEventHandler : INotificationHandler<StartReportActivityEvent>
    {
        private readonly IStepExecutionService _executionService;

        public StartReportActivityEventHandler(IStepExecutionService executionService)
        {
            _executionService = executionService;
        }

        public async Task Handle(StartReportActivityEvent notification, CancellationToken cancellationToken)
        {
            await _executionService.UpdateStatusAsync(notification.StepExecutionId, ExecutionStatus.Running);
        }
    }
}
