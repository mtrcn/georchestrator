using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;
using StackExchange.Redis;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisStepExecutionRepository : IStepExecutionRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string KeyPrefix = "step_execution:";
        private const string ListKey = "step_executions";
        private const string ParentIndexKey = "step_execution_parent:";

        public RedisStepExecutionRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<StepExecution> CreateAsync(StepExecution stepExecution)
        {
            var db = _redis.GetDatabase();
            stepExecution.Id = $"{stepExecution.JobId}-{stepExecution.Iteration}-{stepExecution.StepId}";
            var key = $"{KeyPrefix}{stepExecution.Id}";

            var serializedData = JsonSerializer.Serialize(stepExecution);
            await db.StringSetAsync(key, serializedData);
            await db.ListRightPushAsync(ListKey, stepExecution.Id);

            if (!string.IsNullOrEmpty(stepExecution.ParentStepExecutionId))
            {
                var parentIndexKey = $"{ParentIndexKey}{stepExecution.ParentStepExecutionId}";
                await db.ListRightPushAsync(parentIndexKey, stepExecution.Id);
            }

            return stepExecution;
        }

        public async Task<StepExecution> GetAsync(string jobId, int iteration, string stepId)
        {
            var id = $"{jobId}-{iteration}-{stepId}";
            return await GetByIdAsync(id);
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                throw new RepositoryException($"Step execution with ID {id} not found");

            var stepExecutionData = JsonSerializer.Deserialize<StepExecution>(value.ToString());
            stepExecutionData.Status = status;

            var serializedData = JsonSerializer.Serialize(stepExecutionData);
            await db.StringSetAsync(key, serializedData);
        }

        public async Task<StepExecution> GetByIdAsync(string id)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                return null;

            try
            {
                return JsonSerializer.Deserialize<StepExecution>(value.ToString());
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to deserialize step execution data: {ex.Message}");
            }
        }

        public async Task<List<StepExecution>> GetByParentId(string parentStepExecutionId)
        {
            var db = _redis.GetDatabase();
            var result = new List<StepExecution>();
            var parentIndexKey = $"{ParentIndexKey}{parentStepExecutionId}";

            var stepExecutionIds = await db.ListRangeAsync(parentIndexKey);
            foreach (var stepExecutionId in stepExecutionIds)
            {
                var stepExecution = await GetByIdAsync(stepExecutionId.ToString());
                if (stepExecution != null)
                {
                    result.Add(stepExecution);
                }
            }

            return result;
        }

        public async Task UpdateCompletedIterationAsync(string id, int completedIteration)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                throw new RepositoryException($"Step execution with ID {id} not found");

            var stepExecutionData = JsonSerializer.Deserialize<StepExecution>(value.ToString());
            stepExecutionData.CompletedIteration = completedIteration;

            var serializedData = JsonSerializer.Serialize(stepExecutionData);
            await db.StringSetAsync(key, serializedData);
        }

        public async Task UpdateContainerAsync(string id, Container container)
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}{id}";

            var value = await db.StringGetAsync(key);
            if (value.IsNull)
                throw new RepositoryException($"Step execution with ID {id} not found");

            var stepExecutionData = JsonSerializer.Deserialize<StepExecution>(value.ToString());
            stepExecutionData.ContainerId = container.Id;
            stepExecutionData.ContainerDetails = JsonSerializer.Serialize(container);

            var serializedData = JsonSerializer.Serialize(stepExecutionData);
            await db.StringSetAsync(key, serializedData);
        }
    }
} 