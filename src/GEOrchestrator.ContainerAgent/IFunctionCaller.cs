using System.Threading.Tasks;

namespace GEOrchestrator.ContainerAgent
{
    public interface IFunctionCaller
    {
        Task RunAsync();
    }
}