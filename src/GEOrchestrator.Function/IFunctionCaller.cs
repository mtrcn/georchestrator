using System.Threading.Tasks;

namespace GEOrchestrator.Function
{
    public interface IFunctionCaller
    {
        Task RunAsync();
    }
}