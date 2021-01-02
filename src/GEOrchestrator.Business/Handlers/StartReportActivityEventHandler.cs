using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Enums;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class StartReportActivityEventHandler : INotificationHandler<StartReportActivityEvent>
    {
        private readonly IExecutionService _executionService;

        public StartReportActivityEventHandler(IExecutionService executionService)
        {
            _executionService = executionService;
        }

        public async Task Handle(StartReportActivityEvent notification, CancellationToken cancellationToken)
        {
            await _executionService.UpdateExecutionStepStatusAsync(notification.ExecutionId, notification.StepId, ExecutionStatus.Running);
        }
    }
}
