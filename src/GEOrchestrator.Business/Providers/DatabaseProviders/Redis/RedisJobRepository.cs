using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Jobs;
using StackExchange.Redis;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisJobRepository : IJobRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string KeyPrefix = "job:";
        private const string ListKey = "jobs";

        public RedisJobRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task CreateAsync(Job job)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{job.Id}";

            var serializedData = JsonSerializer.Serialize(job);
            await db.StringSetAsync(key, serializedData);
            await db.ListRightPushAsync(ListKey, job.Id);
        }

        public async Task<Job> GetByIdAsync(string id)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                return null;

            try
            {
                return JsonSerializer.Deserialize<Job>(value.ToString());
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to deserialize job data: {ex.Message}");
            }
        }

        public async Task UpdateStatusAsync(string id, string status, string message, DateTime updated)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                throw new RepositoryException($"Job with ID {id} not found");

            var job = JsonSerializer.Deserialize<Job>(value.ToString());
            job.Status = status;
            job.Message = message;
            job.Updated = updated;

            var serializedData = JsonSerializer.Serialize(job);
            await db.StringSetAsync(key, serializedData);
        }

        public async Task UpdateStartedAsync(string id, DateTime started)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                throw new RepositoryException($"Job with ID {id} not found");

            var job = JsonSerializer.Deserialize<Job>(value.ToString());
            job.Started = started;
            job.Updated = DateTime.UtcNow;

            var serializedData = JsonSerializer.Serialize(job);
            await db.StringSetAsync(key, serializedData);
        }

        public async Task UpdateFinishedAsync(string id, DateTime finished)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                throw new RepositoryException($"Job with ID {id} not found");

            var jobData = JsonSerializer.Deserialize<dynamic>(value.ToString());
            jobData.Finished = finished.Ticks;
            jobData.Updated = DateTime.UtcNow.Ticks;

            var serializedData = JsonSerializer.Serialize(jobData);
            await db.StringSetAsync(key, serializedData);
        }

        public async Task<List<Job>> GetAllAsync()
        {
            var db = _redis.GetDatabase();
            var result = new List<Job>();

            var jobIds = await db.ListRangeAsync(ListKey);
            foreach (var jobId in jobIds)
            {
                var job = await GetByIdAsync(jobId.ToString());
                if (job != null)
                {
                    result.Add(job);
                }
            }

            return result;
        }
    }
} 