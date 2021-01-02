using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Domain.Models.Parameters;

namespace GEOrchestrator.Business.Repositories.Parameters
{
    public class DynamoDbParameterRepository : IParameterRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-parameters";

        public DynamoDbParameterRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task<Parameter> GetAsync(string workflowRunId, string stepId, string name)
        {
            var req = new GetItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue($"{workflowRunId}-{stepId}-{name}") }
                }
            };
            var response = await _amazonDynamoDb.GetItemAsync(req);

            if (response.HttpStatusCode == HttpStatusCode.NotFound)
                throw new WorkflowNotFoundException();

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Workflow cannot be queried successfully.");

            var result = new Parameter
            {
                WorkflowRunId = workflowRunId,
                StepId = stepId,
                Name = name,
                StoragePath = response.Item["storage_path"].S
            };

            return result;
        }

        public async Task AddAsync(Parameter parameter)
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue($"{parameter.WorkflowRunId}-{parameter.StepId}-{parameter.Name}") },
                    {"workflow_run_id", new AttributeValue(parameter.WorkflowRunId) },
                    {"step_id", new AttributeValue(parameter.StepId) },
                    {"parameter_name", new AttributeValue(parameter.Name) },
                    {"storage_path", new AttributeValue(parameter.StoragePath) }
                }
            };
            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }
    }
}
