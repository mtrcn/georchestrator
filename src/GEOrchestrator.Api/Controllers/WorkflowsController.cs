using System.Threading.Tasks;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Models.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace GEOrchestrator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowsController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody]Workflow workflow)
        {
            var validationMessages = await _workflowService.Register(workflow);
            if (validationMessages.Count > 0)
            {
                foreach (var validationMessage in validationMessages)
                {
                    ModelState.AddModelError("validationResults", validationMessage);
                }

                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
