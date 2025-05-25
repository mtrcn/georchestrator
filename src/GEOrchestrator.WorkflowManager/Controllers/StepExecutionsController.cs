using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Dtos;

namespace GEOrchestrator.WorkflowManager.Controllers
{
    [ApiController]
    [Route("jobs/{jobId}/step-executions")]
    public class StepExecutionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StepExecutionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{stepExecutionId}/activities")]
        public async Task<IActionResult> ActivityAsync(string jobId, string stepExecutionId, [FromBody] StepExecutionActivityDto stepExecutionActivityDto)
        {
            await _mediator.Publish(new StepExecutionEvent(jobId, stepExecutionId, stepExecutionActivityDto.Type, stepExecutionActivityDto.Payload));
            return NoContent();
        }

        [HttpGet("{stepExecutionId}/inputs")]
        public async Task<IActionResult> InputsAsync(string jobId, string stepExecutionId)
        {
            var response = await _mediator.Send(new StepExecutionInputsRequest(jobId, stepExecutionId));
            return Ok(response);
        }

        [HttpPost("{stepExecutionId}/outputs")]
        public async Task<IActionResult> SaveArtifactOutputsAsync(string jobId, string stepExecutionId, [FromBody] SendOutputActivityDto sendOutputActivityDto)
        {
            var response = await _mediator.Send(new SendOutputRequest(jobId, stepExecutionId, sendOutputActivityDto));
            if (string.IsNullOrEmpty(response))
                return NoContent();
            return Ok(response);
        }
    }
}
