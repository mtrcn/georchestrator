using System;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Jobs;

namespace GEOrchestrator.Business.Repositories
{
    public interface IJobRepository
    {
        Task CreateAsync(Job job);
        Task<Job> GetByIdAsync(string id);
        Task UpdateStatusAsync(string id, string status, string message, DateTime updated);
        Task UpdateStartedAsync(string id, DateTime started);
        Task UpdateFinishedAsync(string id, DateTime finished);
    }
}
