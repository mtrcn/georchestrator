using System.Threading.Tasks;
using GEOrchestrator.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace GEOrchestrator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> SaveAsync([FromBody] Domain.Models.Tasks.Task task)
        {
            await _taskService.SaveAsync(task);
            return NoContent();
        }
    }
}
