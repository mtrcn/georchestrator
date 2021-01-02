using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Repositories.Executions
{
    public class DynamoDbExecutionRepository : IExecutionRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-executions";

        public DynamoDbExecutionRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task Create(Execution execution)
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue(execution.Id) },
                    {"workflow_run_id", new AttributeValue(execution.WorkflowRunId) },
                    {"steps", new AttributeValue(JsonSerializer.Serialize(execution.Steps)) },
                    {"workflow_name", new AttributeValue(execution.WorkflowName) },
                    {"workflow_version", new AttributeValue { N = execution.WorkflowVersion.ToString() } },
                    {"execution_status", new AttributeValue(execution.Status) },
                    {"started_on", new AttributeValue { N = execution.StartedOn.Ticks.ToString() } }
                }
            };

            if (!string.IsNullOrEmpty(execution.ParentExecutionId))
                putItemRequest.Item.Add("parent_execution_id", new AttributeValue(execution.ParentExecutionId));

            if (!string.IsNullOrEmpty(execution.ParentStepId))
                putItemRequest.Item.Add("parent_step_id", new AttributeValue(execution.ParentStepId));

            if (execution.Iteration != null)
                putItemRequest.Item.Add("iteration", new AttributeValue(JsonSerializer.Serialize(execution.Iteration)));

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task<Execution> GetByIdAsync(string id)
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

        public async Task<IEnumerable<Execution>> GetByParentId(string parentExecutionId)
        {
            var queryRequest = new QueryRequest
            {
                TableName = TableName,
                IndexName = "parent_execution_id_index",
                KeyConditionExpression = "parent_execution_id = :parent_execution_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":parent_execution_id", new AttributeValue { S =  parentExecutionId }}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(queryRequest);
            //projected attributes: id, parent_execution_id, parent_step_id, execution_status, id, workflow_run_id
            return response.Items.Select(item => new Execution {
                Id = item["id"].S,
                ParentExecutionId =  item.ContainsKey("parent_execution_id") ? item["parent_execution_id"].S : null,
                ParentStepId =  item.ContainsKey("parent_step_id") ? item["parent_step_id"].S : null,
                WorkflowRunId = item["workflow_run_id"].S,
                Status = item["execution_status"].S
            }).ToList();
        }

        public async Task UpdateStatus(string id, string status)
        {
            var updateItemRequest = new UpdateItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET execution_status = :execution_status",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":execution_status" , new AttributeValue { S = status } }
                }
            };

            try
            {
                await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
            }
            catch(ConditionalCheckFailedException) {}
        }

        private static Execution MapItem(Dictionary<string, AttributeValue> item)
        {
            var result = new Execution
            {
                Id = item["id"].S,
                ParentExecutionId =  item.ContainsKey("parent_execution_id") ? item["parent_execution_id"].S : null,
                ParentStepId =  item.ContainsKey("parent_step_id") ? item["parent_step_id"].S : null,
                WorkflowRunId = item["workflow_run_id"].S,
                WorkflowName = item["workflow_name"].S,
                WorkflowVersion = int.Parse(item["workflow_version"].N),
                Steps = JsonSerializer.Deserialize<List<WorkflowStep>>(item["steps"].S),
                Iteration = item.ContainsKey("iteration") ? JsonSerializer.Deserialize<ExecutionIteration>(item["iteration"].S) : null,
                Status = item["execution_status"].S,
                StartedOn = new DateTime(long.Parse(item["started_on"].N)),
                CompletedOn = item.ContainsKey("completed_on") ? new DateTime(long.Parse(item["completed_on"].N)) : (DateTime?)null
            };

            return result;
        }
    }
}
