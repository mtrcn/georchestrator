using GEOrchestrator.Domain.Dtos;
using GEOrchestrator.Domain.Models.Jobs;
using GEOrchestrator.Domain.Models.Workflows;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public interface IJobService
    {
        Task<Job> CreateJobAsync(Workflow workflow);
        Task<Job> GetJobAsync(string jobId);
        Task SaveJobStatusAsync(string jobId, string status, string message);
        Task StartJobAsync(string jobId);
        Task FinishJobAsync(string jobId);
        Task<Dictionary<string, JsonNode>> GetJobOutputAsync(string jobId);
        Task<JobStatusDto> GetJobStatusAsync(string jobId);
    }
}