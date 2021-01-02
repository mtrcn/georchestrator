using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Activities;
using MediatR;

namespace GEOrchestrator.Business.Handlers
{
    public class ExecutionStepActivityRequestHandler : IRequestHandler<ExecutionStepActivityRequest, object>
    {
        private readonly IMediator _mediator;
        private readonly IExecutionService _stepExecuterService;

        public ExecutionStepActivityRequestHandler(IMediator mediator, IExecutionService stepExecuterService)
        {
            _mediator = mediator;
            _stepExecuterService = stepExecuterService;
        }

        public async Task<object> Handle(ExecutionStepActivityRequest notification, CancellationToken cancellationToken)
        {
            object result = null;
            var payload = FromBase64(notification.ExecutionStepActivity.ActivityPayload);
            var executionStep = await _stepExecuterService.GetExecutionStepByExecutionIdAndStepId(notification.ExecutionStepActivity.ExecutionId, notification.ExecutionStepActivity.StepId);

            switch (notification.ExecutionStepActivity.ActivityType)
            {
                case nameof(StartReportActivity):
                    await _mediator.Publish(new StartReportActivityEvent(executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<StartReportActivity>(payload)), cancellationToken);
                    break;
                case nameof(ReceiveParameterActivity):
                    result = await _mediator.Send(new ReceiveParameterActivityRequest(executionStep.WorkflowRunId, executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<ReceiveParameterActivity>(payload)), cancellationToken);
                    break;
                case nameof(ReceiveArtifactActivity):
                    result = await _mediator.Send(new ReceiveArtifactActivityRequest(executionStep.WorkflowRunId, executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<ReceiveArtifactActivity>(payload)), cancellationToken);
                    break;
                case nameof(SendArtifactActivity):
                    result = await _mediator.Send(new SendArtifactActivityRequest(executionStep.WorkflowRunId, executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<SendArtifactActivity>(payload)), cancellationToken);
                    break;
                case nameof(SendParameterActivity):
                    result = await _mediator.Send(new SendParameterActivityEvent(executionStep.WorkflowRunId, executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<SendParameterActivity>(payload)), cancellationToken);
                    break;
                case nameof(SendInformationMessageActivity):
                    await _mediator.Publish(new SendInformationMessageActivityEvent(executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<SendInformationMessageActivity>(payload)), cancellationToken);
                    break;
                case nameof(SendErrorMessageActivity):
                    await _mediator.Publish(new SendErrorMessageActivityEvent(executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<SendErrorMessageActivity>(payload)), cancellationToken);
                    break;
                case nameof(CompletedReportActivity):
                    await _mediator.Publish(new CompletedReportActivityEvent(executionStep.ExecutionId, executionStep.StepId, JsonSerializer.Deserialize<CompletedReportActivity>(payload)), cancellationToken);
                    break;
            }

            return result;
        }

        private static string FromBase64(string value)
        {
            var valueAsBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueAsBytes);
        }
    }
}
