using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Parameters;
using StackExchange.Redis;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.Redis
{
    public class RedisParameterRepository : IParameterRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string KeyPrefix = "parameter:";
        private const string ListKey = "parameters";
        private const string JobIndexKey = "job_parameters:";
        private const string StepIndexKey = "step_parameters:";

        public RedisParameterRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task AddAsync(Parameter parameter)
        {
            var db = _redis.GetDatabase();
            var parameterId = Guid.NewGuid().ToString();
            var key = $"{KeyPrefix}{parameterId}";

            var serializedData = JsonSerializer.Serialize(parameter);
            await db.StringSetAsync(key, serializedData);
            await db.ListRightPushAsync(ListKey, parameterId);

            var jobIndexKey = $"{JobIndexKey}{parameter.JobId}";
            await db.ListRightPushAsync(jobIndexKey, parameterId);

            if (!string.IsNullOrEmpty(parameter.StepId))
            {
                var stepIndexKey = $"{StepIndexKey}{parameter.JobId}:{parameter.StepId}";
                await db.ListRightPushAsync(stepIndexKey, parameterId);
            }
        }

        public async Task<List<Parameter>> GetAsync(string jobId, string name, string stepId = null)
        {
            var db = _redis.GetDatabase();
            var result = new List<Parameter>();
            var indexKey = string.IsNullOrEmpty(stepId) 
                ? $"{JobIndexKey}{jobId}"
                : $"{StepIndexKey}{jobId}:{stepId}";

            var parameterIds = await db.ListRangeAsync(indexKey);
            foreach (var parameterId in parameterIds)
            {
                var key = $"{KeyPrefix}{parameterId}";
                var value = await db.StringGetAsync(key);
                if (!value.IsNull)
                {
                    try
                    {
                        var parameter = JsonSerializer.Deserialize<Parameter>(value.ToString());

                        if (parameter.Name == name)
                        {
                            result.Add(parameter);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new RepositoryException($"Failed to deserialize parameter data: {ex.Message}");
                    }
                }
            }

            return result.OrderBy(p => p.Index).ToList();
        }

        public async Task<List<Parameter>> GetByReference(string jobId, string referenceValue)
        {
            var referenceType = referenceValue.GetReferenceType();

            switch (referenceType.ToLowerInvariant())
            {
                case "input":
                    var inputParameterName = referenceValue.ParseInputReferenceValue();
                    return await GetAsync(jobId, inputParameterName);
                case "step":
                    var (stepId, parameterName) = referenceValue.ParseStepReferenceValue();
                    return await GetAsync(jobId, parameterName, stepId);
            }

            return new List<Parameter>();
        }
    }
} 