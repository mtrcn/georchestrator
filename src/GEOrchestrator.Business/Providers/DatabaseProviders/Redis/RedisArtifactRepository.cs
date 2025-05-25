using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Artifacts;
using StackExchange.Redis;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisArtifactRepository : IArtifactRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string KeyPrefix = "artifact:";
        private const string ListKey = "artifacts";
        private const string JobIndexKey = "job_artifacts:";
        private const string StepIndexKey = "step_artifacts:";

        public RedisArtifactRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task AddAsync(Artifact artifact)
        {
            var db = _redis.GetDatabase();
            var artifactId = Guid.NewGuid().ToString();
            var key = $"{KeyPrefix}{artifactId}";

            var serializedData = JsonSerializer.Serialize(artifact);
            await db.StringSetAsync(key, serializedData);
            await db.ListRightPushAsync(ListKey, artifactId);

            var jobIndexKey = $"{JobIndexKey}{artifact.JobId}";
            await db.ListRightPushAsync(jobIndexKey, artifactId);

            var stepIndexKey = $"{StepIndexKey}{artifact.JobId}:{artifact.StepId}";
            await db.ListRightPushAsync(stepIndexKey, artifactId);
        }

        public async Task<List<Artifact>> GetAsync(string jobId, string stepId, string name)
        {
            var db = _redis.GetDatabase();
            var result = new List<Artifact>();
            var stepIndexKey = $"{StepIndexKey}{jobId}:{stepId}";

            var artifactIds = await db.ListRangeAsync(stepIndexKey);
            foreach (var artifactId in artifactIds)
            {
                var key = $"{KeyPrefix}{artifactId}";
                var value = await db.StringGetAsync(key);
                if (!value.IsNull)
                {
                    try
                    {
                        var artifact = JsonSerializer.Deserialize<Artifact>(value.ToString());

                        if (string.IsNullOrEmpty(name) || artifact.Name == name)
                        {
                            result.Add(artifact);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new RepositoryException($"Failed to deserialize artifact data: {ex.Message}");
                    }
                }
            }

            return result;
        }
    }
} 