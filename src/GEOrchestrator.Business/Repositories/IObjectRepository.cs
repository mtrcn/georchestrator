using System.Threading.Tasks;

namespace GEOrchestrator.Business.Repositories
{
    public interface IObjectRepository
    {
        Task<string> GetSignedUrlForDownloadAsync(string path);
        Task<string> GetSignedUrlForUploadAsync(string path);
    }
}
