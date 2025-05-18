using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb
{
    public class DynamoDbWorkflowRepository : IWorkflowRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-workflows";

        public DynamoDbWorkflowRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task RegisterAsync(Workflow workflow)
        {
            var putItemRequest = new PutItemRequest()
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"workflow_name", new AttributeValue(workflow.Name) },
                    {"version", new AttributeValue { N = workflow.Version.ToString() } },
                    {"definition", new AttributeValue(JsonSerializer.Serialize(workflow)) }
                }
            };

            if (!string.IsNullOrEmpty(workflow.Description))
                putItemRequest.Item["description"] = new AttributeValue(workflow.Description);

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task<Workflow> GetByNameAsync(string workflowName)
        {
            var getItemRequest = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "workflow_name", new AttributeValue(workflowName) }
                }
            };
            var response = await _amazonDynamoDb.GetItemAsync(getItemRequest);

            if (response.Item == null || response.Item.Count == 0)
                return null;

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Workflow cannot be queried successfully.");

            var result = JsonSerializer.Deserialize<Workflow>(response.Item["definition"].S);

            return result;
        }

        public async Task<List<Workflow>> GetAllAsync()
        {
            var result = new List<Workflow>();
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;

            do
            {
                var response =
                    await _amazonDynamoDb.ScanAsync(new ScanRequest(TableName) {ExclusiveStartKey = lastKeyEvaluated});

                foreach (var item in response.Items)
                {
                    result.Add(JsonSerializer.Deserialize<Workflow>(item["definition"].S));
                }

                lastKeyEvaluated = response.LastEvaluatedKey;

            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

            return result;
        }
    }
}
