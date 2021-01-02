using System.Threading.Tasks;

namespace GEOrchestrator.Business.Repositories.Objects
{
    public interface IObjectRepository
    {
        public Task<byte[]> GetAsync(string path);
        public Task AddAsync(string path, byte[] content);
        Task<string> GetSignedUrlForDownloadAsync(string path);
        Task<string> GetSignedUrlForUploadAsync(string path);
    }
}
