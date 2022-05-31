using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public interface ITaskService
    {
        Task<Domain.Models.Tasks.Task> GetTaskByNameAsync(string name);
        Task SaveAsync(Domain.Models.Tasks.Task task);
    }
}
