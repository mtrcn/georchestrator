using GEOrchestrator.Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GEOrchestrator.WorkflowManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetAsync(string jobId)
        {
            var jobStatus = await _jobService.GetJobStatusAsync(jobId);

            if (jobStatus == null)
                return NotFound();

            return Ok(jobStatus);
        }

        [HttpGet("{jobId}/results")]
        public async Task<IActionResult> GetResultsAsync(string jobId)
        {
            var jobOutput = await _jobService.GetJobOutputAsync(jobId);

            if (jobOutput == null)
                return NotFound();

            return Ok(jobOutput);
        }

        [HttpGet("{jobId}/logs")]
        public async Task<IActionResult> GetLogsAsync(string jobId)
        {
            var jobOutput = await _jobService.GetLogsAsync(jobId);

            return Ok(jobOutput);
        }
    }
}
