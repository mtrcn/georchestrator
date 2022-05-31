using System.Threading.Tasks;

namespace GEOrchestrator.Business.Repositories
{
    public interface ITaskRepository
    {
        Task SaveAsync(Domain.Models.Tasks.Task task);
        Task<Domain.Models.Tasks.Task> GetByNameAsync(string name);
        Task RemoveAsync(string id);
    }
}
