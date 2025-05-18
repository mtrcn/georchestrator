using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Containers;
using GEOrchestrator.Domain.Models.Executions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb
{
    public class DynamoDbStepExecutionRepository : IStepExecutionRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-step-executions";

        public DynamoDbStepExecutionRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task<StepExecution> CreateAsync(StepExecution stepExecution)
        {
            stepExecution.Id = $"{stepExecution.JobId}-{stepExecution.Iteration}-{stepExecution.StepId}";
            var putItemRequest = new PutItemRequest()
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue(stepExecution.Id) },
                    {"job_id", new AttributeValue(stepExecution.JobId) },
                    {"iteration", new AttributeValue { N = stepExecution.Iteration.ToString()} },
                    {"total_iteration", new AttributeValue(){ N = stepExecution.TotalIteration.ToString()} },
                    {"completed_iteration", new AttributeValue(){ N = stepExecution.CompletedIteration.ToString()} },
                    {"task", new AttributeValue(stepExecution.Task)},
                    {"step_id", new AttributeValue(stepExecution.StepId) },
                    {"step_status", new AttributeValue(stepExecution.Status) }
                },
                ConditionExpression = "attribute_not_exists(id)"
            };

            if (!string.IsNullOrEmpty(stepExecution.ParentStepExecutionId))
                putItemRequest.Item["parent_step_execution_id"] = new AttributeValue(stepExecution.ParentStepExecutionId);

            if (!string.IsNullOrEmpty(stepExecution.ContainerId))
                putItemRequest.Item["container_id"] = new AttributeValue(stepExecution.ContainerId);

            if (!string.IsNullOrEmpty(stepExecution.ContainerDetails))
                putItemRequest.Item["container_details"] = new AttributeValue(JsonSerializer.Serialize(stepExecution.ContainerDetails));

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
            return stepExecution;
        }

        public async Task<List<StepExecution>> GetByParentId(string parentStepExecutionId)
        {
            var queryRequest = new QueryRequest
            {
                TableName = TableName,
                IndexName = "parent_step_execution_id_index",
                KeyConditionExpression = "parent_step_execution_id = :parent_step_execution_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":parent_step_execution_id", new AttributeValue { S =  parentStepExecutionId }}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(queryRequest);
            //projected attributes: id, parent_execution_id, parent_step_id, execution_status, id, workflow_run_id
            return response.Items.Select(item => new StepExecution
            {
                JobId = item["job_id"].S,
                ParentStepExecutionId = item["parent_step_execution_id"].S,
                CompletedIteration = int.Parse(item["completed_iteration"].N),
                TotalIteration = int.Parse(item["total_iteration"].N),
                Iteration = int.Parse(item["iteration"].N),
                Task = item["task"].S,
                StepId = item["step_id"].S,
                Status = item["step_status"].S,
                ContainerId = item["container_id"].S,
                ContainerDetails = item.ContainsKey("container_details") ? item["container_details"].S : null
            }).ToList();
        }

        public async Task UpdateCompletedIterationAsync(string id, int completedIteration)
        {
            var updateItemRequest = new UpdateItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET completed_iteration = :completed_iteration",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    { ":completed_iteration" , new AttributeValue() { N = completedIteration.ToString() } }
                }
            };
            await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
        }

        public async Task UpdateContainerAsync(string id, Container container)
        {
            var updateItemRequest = new UpdateItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET container_id = :container_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    { ":container_id" , new AttributeValue(container.Id) }
                }
            };

            if (!string.IsNullOrEmpty(container.Details))
            {
                updateItemRequest.UpdateExpression += ", container_details = :container_details";
                updateItemRequest.ExpressionAttributeValues.Add(":container_details", new AttributeValue(JsonSerializer.Serialize(container.Details)));
            }

            await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            var updateItemRequest = new UpdateItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                },
                UpdateExpression = "SET step_status = :step_status",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    { ":step_status" , new AttributeValue() { S = status } }
                }
            };
            await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
        }

        public async Task<StepExecution> GetAsync(string jobId, int iteration, string stepId)
        {
            var req = new GetItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue($"{jobId}-{iteration}-{stepId}") }
                }
            };

            var response = await _amazonDynamoDb.GetItemAsync(req);

            if (response.HttpStatusCode == HttpStatusCode.NotFound)
                throw new WorkflowNotFoundException();

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Execution step cannot be queried successfully.");

            return new StepExecution
            {
                Id = response.Item["id"].S,
                JobId = response.Item["job_id"].S,
                ParentStepExecutionId = response.Item["parent_step_execution_id"].S,
                CompletedIteration = int.Parse(response.Item["completed_iteration"].N),
                TotalIteration = int.Parse(response.Item["total_iteration"].N),
                Iteration = int.Parse(response.Item["iteration"].N),
                StepId = response.Item["step_id"].S,
                Status = response.Item["step_status"].S,
                Task = response.Item["task"].S,
                ContainerId = response.Item["container_id"].S,
                ContainerDetails = response.Item.ContainsKey("container_details") ? response.Item["container_details"].S : null
            };
        }

        public async Task<StepExecution> GetByIdAsync(string id)
        {
            var req = new GetItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue(id) }
                }
            };

            var response = await _amazonDynamoDb.GetItemAsync(req);

            if (response.HttpStatusCode == HttpStatusCode.NotFound)
                throw new WorkflowNotFoundException();

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Execution step cannot be queried successfully.");

            return new StepExecution
            {
                Id = response.Item["id"].S,
                JobId = response.Item["job_id"].S,
                ParentStepExecutionId = response.Item.ContainsKey("parent_step_execution_id") ? response.Item["parent_step_execution_id"].S : null,
                CompletedIteration = int.Parse(response.Item["completed_iteration"].N),
                TotalIteration = int.Parse(response.Item["total_iteration"].N),
                Iteration = int.Parse(response.Item["iteration"].N),
                Task = response.Item["task"].S,
                StepId = response.Item["step_id"].S,
                Status = response.Item["step_status"].S,
                ContainerId = response.Item.ContainsKey("container_id") ? response.Item["container_id"].S : null,
                ContainerDetails = response.Item.ContainsKey("container_details") ? response.Item["container_details"].S : null
            };
        }
    }
}
