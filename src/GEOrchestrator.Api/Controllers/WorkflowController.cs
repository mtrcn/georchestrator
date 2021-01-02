using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Domain.Models.Executions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GEOrchestrator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkflowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("run/{workflowName}")]
        public async Task<IActionResult> RunAsync(string workflowName)
        {
            await _mediator.Publish(new RunWorkflowEvent(workflowName));
            return Ok();
        }

        [HttpPost("step/activity")]
        public async Task<IActionResult> ExecuteAsync(ExecutionStepActivity executionStepActivity)
        {
            var result = await _mediator.Send(new ExecutionStepActivityRequest(executionStepActivity));
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromForm] string definition, CancellationToken cancellationToken)
        {
            await _mediator.Publish(new RegisterWorkflowEvent(definition), cancellationToken);
            return Ok();
        }
    }
}
