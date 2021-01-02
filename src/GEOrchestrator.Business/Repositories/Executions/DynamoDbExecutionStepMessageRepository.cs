using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Repositories.Executions
{
    public class DynamoDbExecutionStepMessageRepository : IExecutionStepMessageRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-execution-messages";

        public DynamoDbExecutionStepMessageRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task AddAsync(ExecutionStepMessage executionMessage)
        {
            var putItemRequest = new PutItemRequest()
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue($"{executionMessage.ExecutionId}-{executionMessage.StepId}") },
                    {"execution_id", new AttributeValue(executionMessage.ExecutionId) },
                    {"step_id", new AttributeValue(executionMessage.StepId) },
                    {"sent_on", new AttributeValue{ N = executionMessage.SentOn.Ticks.ToString() } },
                    {"message_type", new AttributeValue(executionMessage.Type) },
                    {"message", new AttributeValue(executionMessage.Message) }
                }
            };

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }
    }
}
