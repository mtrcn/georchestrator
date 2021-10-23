using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Jobs;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb
{
    public class DynamoDbJobRepository : IJobRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-jobs";

        public DynamoDbJobRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task CreateAsync(Job job)
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue(job.Id) },
                    {"workflow", new AttributeValue(JsonConvert.SerializeObject(job.Workflow)) },
                    {"workflow_name", new AttributeValue(job.WorkflowName) },
                    {"workflow_version", new AttributeValue { N = job.WorkflowVersion.ToString() } },
                    {"job_status", new AttributeValue(job.Status) },
                    {"created", new AttributeValue { N = job.Created.Ticks.ToString() } },
                    {"updated", new AttributeValue { N = job.Updated.Ticks.ToString() } }
                }
            };

            if (!string.IsNullOrEmpty(job.Message))
                putItemRequest.Item.Add("message", new AttributeValue(job.Message));

            if (job.Started.HasValue)
                putItemRequest.Item.Add("started", new AttributeValue { N = job.Started?.Ticks.ToString() });

            if (job.Finished.HasValue)
                putItemRequest.Item.Add("finished", new AttributeValue { N = job.Finished?.Ticks.ToString() });

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task<Job> GetByIdAsync(string id)
        {
            var req = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                }
            };
            var response = await _amazonDynamoDb.GetItemAsync(req);

            if (response.HttpStatusCode == HttpStatusCode.NotFound)
                throw new ExecutionNotFoundException();

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Execution cannot be queried successfully.");

            return MapItem(response.Item);
        }

        public async Task UpdateStatusAsync(string id, string status, string message, DateTime updated)
        {
            var updateItemRequest = new UpdateItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET job_status = :job_status, message = :message, updated = :updated",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":job_status" , new AttributeValue { S = status } },
                    { ":message", new AttributeValue { S = message } },
                    { ":updated", new AttributeValue { N = updated.Ticks.ToString() } },
                }
            };

            try
            {
                await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
            }
            catch(ConditionalCheckFailedException) {}
        }

        public async Task UpdateStartedAsync(string id, DateTime started)
        {
            var updateItemRequest = new UpdateItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET started = :started",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":started", new AttributeValue { N = started.Ticks.ToString() } }
                }
            };

            try
            {
                await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
            }
            catch (ConditionalCheckFailedException) { }
        }

        public async Task UpdateFinishedAsync(string id, DateTime finished)
        {
            var updateItemRequest = new UpdateItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET finished = :finished",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":finished", new AttributeValue { N = finished.Ticks.ToString() } }
                }
            };

            try
            {
                await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
            }
            catch (ConditionalCheckFailedException) { }
        }

        private static Job MapItem(Dictionary<string, AttributeValue> item)
        {
            var result = new Job
            {
                Id = item["id"].S,
                WorkflowName = item["workflow_name"].S,
                WorkflowVersion = int.Parse(item["workflow_version"].N),
                Workflow = JsonSerializer.Deserialize<Domain.Models.Workflows.Workflow>(item["workflow"].S),
                Status = item["job_status"].S,
                Message = item.ContainsKey("message") ? item["message"].S : null,
                Created = new DateTime(long.Parse(item["created"].N)),
                Updated = new DateTime(long.Parse(item["updated"].N)),
                Finished = item.ContainsKey("finished") ? new DateTime(long.Parse(item["finished"].N)) : (DateTime?)null,
                Started = item.ContainsKey("started") ? new DateTime(long.Parse(item["started"].N)) : (DateTime?)null
            };

            return result;
        }
    }
}
