using System;
using System.Collections.Generic;
using System.Linq;
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
                    {"job_id", new AttributeValue(executionMessage.JobId) },
                    {"step_execution_id", new AttributeValue(executionMessage.StepExecutionId) },
                    {"sent_on", new AttributeValue{ N = executionMessage.SentOn.Ticks.ToString() } },
                    {"message_type", new AttributeValue(executionMessage.Type) },
                    {"message", new AttributeValue(executionMessage.Message) }
                }
            };

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task<List<StepExecutionMessage>> GetByJobIdAsync(string jobId)
        {
            var queryRequest = new QueryRequest
            {
                TableName = TableName,
                IndexName = "job_id_index",
                KeyConditionExpression = "job_id = :job_id_index",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":job_id_index", new AttributeValue { S =  jobId }}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(queryRequest);
            return response.Items.Select(item => new StepExecutionMessage
            {
                JobId = item["job_id"].S,
                StepExecutionId = item["step_execution_id"].S,
                SentOn = new DateTime(long.Parse(item["sent_on"].N)),
                Type = item["message_type"].S,
                Message = item["message"].S
            }).OrderBy(r => r.SentOn).ToList();
        }
    }
}
