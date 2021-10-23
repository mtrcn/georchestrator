using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Dtos;

namespace GEOrchestrator.WorkflowManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWorkflowService _workflowService;

        public ProcessesController(IMediator mediator, IWorkflowService workflowService)
        {
            _mediator = mediator;
            _workflowService = workflowService;
        }

        [HttpPost("{processId}/execution")]
        public async Task<IActionResult> ExecuteAsync(string processId, [FromBody] ExecuteDto executeDto)
        {
            var workflow = await _workflowService.GetWorkflowByName(processId);

            if (workflow == null)
                return NotFound();

            var response = await _mediator.Send(new RunJobRequest(workflow, executeDto.Inputs));

            return Created($"jobs/{response.JobId}", response);
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            var workflows = await _workflowService.GetAllAsync();

            var response = workflows.Select(w => new ProcessDto
            {
                Id = w.Name,
                Description = w.Description,
                Version = w.Version.ToString()
            });

            return Ok(response);
        }
    }
}
