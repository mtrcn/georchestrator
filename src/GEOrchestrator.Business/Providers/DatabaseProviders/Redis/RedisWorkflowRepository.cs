using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Workflows;
using StackExchange.Redis;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisWorkflowRepository : IWorkflowRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string KeyPrefix = "workflow:";
        private const string ListKey = "workflows";

        public RedisWorkflowRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task RegisterAsync(Workflow workflow)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{workflow.Name}";

            var serializedData = JsonSerializer.Serialize(workflow);
            await db.StringSetAsync(key, serializedData);
            await db.ListRightPushAsync(ListKey, workflow.Name);
        }

        public async Task<Workflow> GetByNameAsync(string workflowName)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{workflowName}";
            
            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                return null;

            try
            {
                return JsonSerializer.Deserialize<Workflow>(value.ToString());
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to deserialize workflow data: {ex.Message}");
            }
        }

        public async Task<List<Workflow>> GetAllAsync()
        {
            var db = _redis.GetDatabase();
            var result = new List<Workflow>();

            var workflowNames = await db.ListRangeAsync(ListKey);
            foreach (var workflowName in workflowNames)
            {
                var workflow = await GetByNameAsync(workflowName.ToString());
                if (workflow != null)
                {
                    result.Add(workflow);
                }
            }

            return result;
        }
    }
} 