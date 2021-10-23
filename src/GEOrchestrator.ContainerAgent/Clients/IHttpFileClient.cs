using System.IO;
using System.Threading.Tasks;

namespace GEOrchestrator.ContainerAgent.Clients
{
    public interface IHttpFileClient
    {
        Task UploadFile(string url, Stream stream);
        Task DownloadFile(string url, string filePath);
    }
}