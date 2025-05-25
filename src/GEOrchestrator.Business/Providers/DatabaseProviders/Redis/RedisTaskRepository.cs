using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisTaskRepository : ITaskRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string KeyPrefix = "task:";
        private const string ListKey = "tasks";

        public RedisTaskRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SaveAsync(Domain.Models.Tasks.Task task)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{task.Name}";

            var serializedData = JsonSerializer.Serialize(task);
            await db.StringSetAsync(key, serializedData);
            await db.ListRightPushAsync(ListKey, task.Name);
        }

        public async Task<Domain.Models.Tasks.Task> GetByNameAsync(string name)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{name}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                return null;

            try
            {
                return JsonSerializer.Deserialize<Domain.Models.Tasks.Task>(value.ToString());
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to deserialize task data: {ex.Message}");
            }
        }

        public async Task RemoveAsync(string id)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (!value.IsNull)
            {
                await db.KeyDeleteAsync(key);
                await db.ListRemoveAsync(ListKey, id);
            }
        }
    }
} 