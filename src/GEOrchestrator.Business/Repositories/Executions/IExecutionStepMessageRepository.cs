using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Repositories.Executions
{
    public interface IExecutionStepMessageRepository
    {
        Task AddAsync(ExecutionStepMessage executionMessage);
    }
}
