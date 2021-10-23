using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Artifacts;
using System.IO;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public class ArtifactService : IArtifactService
    {
        private readonly IObjectRepository _objectRepository;
        private readonly IArtifactRepository _artifactRepository;

        public ArtifactService(
            IObjectRepositoryFactory objectRepositoryFactory,
            IArtifactRepositoryFactory artifactRepositoryFactory)
        {
            _objectRepository = objectRepositoryFactory.Create();
            _artifactRepository = artifactRepositoryFactory.Create();
        }

        public async Task<string> SaveArtifactAsync(Artifact artifact)
        {
            artifact.StoragePath = Path.Join(
                artifact.JobId,
                artifact.StepId,
                artifact.Index.ToString(),
                artifact.Name);

            artifact.StoragePath = artifact.StoragePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            await _artifactRepository.AddAsync(artifact);

            var uploadUrl = await _objectRepository.GetSignedUrlForUploadAsync(artifact.StoragePath);
            return uploadUrl;
        }
    }
}
