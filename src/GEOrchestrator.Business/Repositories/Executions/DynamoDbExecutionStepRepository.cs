using System.Collections.Generic;
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
    public class DynamoDbExecutionStepRepository : IExecutionStepRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-execution-steps";

        public DynamoDbExecutionStepRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task CreateAsync(ExecutionStep executionStep)
        {
            var putItemRequest = new PutItemRequest()
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue($"{executionStep.ExecutionId}-{executionStep.StepId}") },
                    {"workflow_run_id", new AttributeValue(executionStep.WorkflowRunId) },
                    {"execution_id", new AttributeValue(executionStep.ExecutionId) },
                    {"step_id", new AttributeValue(executionStep.StepId) },
                    {"step_status", new AttributeValue(executionStep.Status) },
                    {"step", new AttributeValue(JsonSerializer.Serialize(executionStep.Step)) },
                    {"container_id", new AttributeValue(executionStep.ContainerId) }
                },
                ConditionExpression = "attribute_not_exists(id)"
            };

            if (!string.IsNullOrEmpty(executionStep.ContainerDetails))
                putItemRequest.Item["container_details"] = new AttributeValue(JsonSerializer.Serialize(executionStep.ContainerDetails));

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task UpdateStatusAsync(string executionId, string stepId, string status)
        {
            var updateItemRequest = new UpdateItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue($"{executionId}-{stepId}") }
                },
                UpdateExpression = "SET step_status = :step_status",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    { ":step_status" , new AttributeValue() { S = status } }
                }
            };

            try
            {
                await _amazonDynamoDb.UpdateItemAsync(updateItemRequest);
            }
            catch(ConditionalCheckFailedException) {}
        }

        public async Task<ExecutionStep> GetByExecutionIdAndStepIdAsync(string executionId, string stepId)
        {
            var req = new GetItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue($"{executionId}-{stepId}") }
                }
            };

            var response = await _amazonDynamoDb.GetItemAsync(req);

            if (response.HttpStatusCode == HttpStatusCode.NotFound)
                throw new WorkflowNotFoundException();

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Execution step container cannot be queried successfully.");

            return new ExecutionStep
            {
                WorkflowRunId = response.Item["workflow_run_id"].S,
                ExecutionId = response.Item["execution_id"].S,
                StepId = response.Item["step_id"].S,
                Status = response.Item["step_status"].S,
                Step = JsonSerializer.Deserialize<WorkflowStep>(response.Item["step"].S),
                ContainerId = response.Item["container_id"].S,
                ContainerDetails = response.Item.ContainsKey("container_details") ? response.Item["container_details"].S : null
            };
        }
    }
}
