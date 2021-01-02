using GEOrchestrator.Business.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task<IActionResult> SaveAsync([FromForm] string definition, CancellationToken cancellationToken)
        {
            await _mediator.Publish(new SaveTaskEvent(definition), cancellationToken);
            return Ok();
        }
    }
}
