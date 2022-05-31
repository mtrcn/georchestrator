using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Parameters;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb
{
    public class DynamoDbParameterRepository : IParameterRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-parameters";

        public DynamoDbParameterRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
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

        public async Task<List<Parameter>> GetAsync(string jobId, string name, string stepId = null)
        {
            var parameterPath = string.IsNullOrEmpty(stepId) ? $"{jobId}-input-{name}" : $"{jobId}-step-{stepId}-{name}";
            var queryRequest = new QueryRequest
            {
                TableName = TableName,
                IndexName = "parameter_path_index",
                KeyConditionExpression = "parameter_path = :parameter_path_index",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":parameter_path_index", new AttributeValue { S =  parameterPath }}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(queryRequest);
            return response.Items.Select(item => new Parameter
            {
                JobId = item["job_id"].S,
                StepId = item.ContainsKey("step_id") ? item["step_id"].S : null,
                Index = int.Parse(item["index"].N),
                Name = item["parameter_name"].S,
                Value = item["value"].S
            }).OrderBy(r => r.Index).ToList();
        }

        public async Task AddAsync(Parameter parameter)
        {
            var parameterPath = string.IsNullOrEmpty(parameter.StepId) ? $"{parameter.JobId}-input-{parameter.Name}" : $"{parameter.JobId}-step-{parameter.StepId}-{parameter.Name}";
            var id = Guid.NewGuid().ToString();
            var putItemRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue(id) },
                    {"parameter_path", new AttributeValue(parameterPath) },
                    {"index", new AttributeValue{ N = parameter.Index.ToString()}},
                    {"job_id", new AttributeValue(parameter.JobId) },
                    {"parameter_name", new AttributeValue(parameter.Name) },
                    {"value", new AttributeValue(parameter.Value) }
                }
            };

            if (!string.IsNullOrEmpty(parameter.StepId))
                putItemRequest.Item.Add("step_id", new AttributeValue(parameter.StepId));

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }
    }
}
