using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Executions;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisStepExecutionMessageRepository : IStepExecutionMessageRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string JobMessagesKeyPrefix = "step_execution_messages:job:";

        public RedisStepExecutionMessageRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task AddAsync(StepExecutionMessage executionMessage)
        {
            var db = _redis.GetDatabase();
            var key = $"{JobMessagesKeyPrefix}{executionMessage.JobId}";
            var serializedData = JsonSerializer.Serialize(executionMessage);
            await db.ListRightPushAsync(key, serializedData);
        }

        public async Task<List<StepExecutionMessage>> GetByJobIdAsync(string jobId)
        {
            var db = _redis.GetDatabase();
            var key = $"{JobMessagesKeyPrefix}{jobId}";
            var serializedMessages = await db.ListRangeAsync(key);

            var messages = new List<StepExecutionMessage>();
            foreach (var serialized in serializedMessages)
            {
                if (!serialized.IsNullOrEmpty)
                {
                    var message = JsonSerializer.Deserialize<StepExecutionMessage>(serialized);
                    if (message != null)
                    {
                        messages.Add(message);
                    }
                }
            }

            return messages.OrderBy(m => m.SentOn).ToList();
        }
    }
}
