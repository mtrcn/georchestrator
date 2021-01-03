using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Task = GEOrchestrator.Domain.Models.Tasks.Task;

namespace GEOrchestrator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> SaveAsync([FromBody] Task task, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Publish(new SaveTaskEvent(task), cancellationToken);
                return Ok();
            }
            catch (TaskValidationException validationException)
            {
                return BadRequest(validationException.Message);
            }
        }
    }
}
