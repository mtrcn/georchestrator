using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Workflows;
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

        [HttpPost("run")]
        public async Task<IActionResult> RunAsync([FromBody]Workflow workflow, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Publish(new RunWorkflowEvent(workflow), cancellationToken);
                return Ok();
            }
            catch (WorkflowValidationException validationException)
            {
                return BadRequest(validationException.Message);
            }
        }

        [HttpPost("step/activity")]
        public async Task<IActionResult> ExecuteAsync(ExecutionStepActivity executionStepActivity)
        {
            var result = await _mediator.Send(new ExecutionStepActivityRequest(executionStepActivity));
            return Ok(result);
        }
    }
}
