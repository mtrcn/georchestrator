using GEOrchestrator.Domain.Models.Parameters;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public interface IParameterService
    {
        Task SaveParameterAsync(Parameter parameter);
    }
}