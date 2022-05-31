using System.Collections.Generic;
using GEOrchestrator.Domain.Models.Jobs;
using GEOrchestrator.Domain.Models.Workflows;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Dtos;
using Newtonsoft.Json.Linq;

namespace GEOrchestrator.Business.Services
{
    public interface IJobService
    {
        Task<Job> CreateJobAsync(Workflow workflow);
        Task<Job> GetJobAsync(string jobId);
        Task SaveJobStatusAsync(string jobId, string status, string message);
        Task StartJobAsync(string jobId);
        Task FinishJobAsync(string jobId);
        Task<Dictionary<string, JToken>> GetJobOutputAsync(string jobId);
        Task<JobStatusDto> GetJobStatusAsync(string jobId);
    }
}