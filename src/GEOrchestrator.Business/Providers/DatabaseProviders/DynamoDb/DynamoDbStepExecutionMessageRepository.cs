using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb
{
    public class DynamoDbStepExecutionMessageRepository : IStepExecutionMessageRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-execution-messages";

        public DynamoDbStepExecutionMessageRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task AddAsync(StepExecutionMessage executionMessage)
        {
            var putItemRequest = new PutItemRequest()
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue(Guid.NewGuid().ToString()) },
                    {"step_execution_id", new AttributeValue(executionMessage.StepExecutionId) },
                    {"sent_on", new AttributeValue{ N = executionMessage.SentOn.Ticks.ToString() } },
                    {"message_type", new AttributeValue(executionMessage.Type) },
                    {"message", new AttributeValue(executionMessage.Message) }
                }
            };

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }
    }
}
