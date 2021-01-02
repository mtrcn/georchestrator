﻿using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Domain.Models.Tasks;
using Task = GEOrchestrator.Domain.Models.Tasks.Task;

namespace GEOrchestrator.Business.Repositories.Tasks
{
    public class DynamoDbTaskRepository : ITaskRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-tasks";

        public DynamoDbTaskRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async System.Threading.Tasks.Task SaveAsync(Task task)
        {
            var putItemRequest = new PutItemRequest()
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"task_name", new AttributeValue(task.Name) },
                    {"description", new AttributeValue(task.Description) },
                    {"image", new AttributeValue(task.Image) },
                    {"inputs", new AttributeValue(JsonSerializer.Serialize(task.Inputs)) },
                    {"outputs", new AttributeValue(JsonSerializer.Serialize(task.Outputs)) }
                }
            };

            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task<Task> GetByNameAsync(string name)
        {
            var req = new GetItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "task_name", new AttributeValue(name) }
                }
            };

            var response = await _amazonDynamoDb.GetItemAsync(req);

            if (response.HttpStatusCode == HttpStatusCode.NotFound)
                throw new TaskNotFoundException();

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new RepositoryException("Task cannot be queried successfully.");

            return new Task
            {
                Name = response.Item["task_name"].S,
                Description = response.Item["description"].S,
                Image = response.Item["image"].S,
                Inputs = JsonSerializer.Deserialize<List<TaskInput>>(response.Item["inputs"].S),
                Outputs = JsonSerializer.Deserialize<List<TaskOutput>>(response.Item["outputs"].S)
            };
        }

        public async System.Threading.Tasks.Task RemoveAsync(string name)
        {
            var request = new DeleteItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "task_name", new AttributeValue(name) }
                }
            };

            await _amazonDynamoDb.DeleteItemAsync(request);
        }
    }
}
