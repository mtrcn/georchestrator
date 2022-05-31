using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Artifacts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Providers.DatabaseProviders.DynamoDb
{
    public class DynamoDbArtifactRepository : IArtifactRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-artifacts";

        public DynamoDbArtifactRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task AddAsync(Artifact artifact)
        {
            var artifactPath = $"{artifact.JobId}-step-{artifact.StepId}-{artifact.Name}";
            var putItemRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue(Guid.NewGuid().ToString())},
                    {"job_id", new AttributeValue(artifact.JobId) },
                    {"artifact_path", new AttributeValue(artifactPath) },
                    {"index", new AttributeValue{N = artifact.Index.ToString()}},
                    {"step_id", new AttributeValue(artifact.StepId) },
                    {"artifact_name", new AttributeValue(artifact.Name) },
                    {"storage_path", new AttributeValue(artifact.StoragePath) }
                }
            };
            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }

        public async Task<List<Artifact>> GetAsync(string jobId, string stepId, string name)
        {
            var artifactPath = $"{jobId}-step-{stepId}-{name}";
            var queryRequest = new QueryRequest
            {
                TableName = TableName,
                IndexName = "artifact_path_index",
                KeyConditionExpression = "artifact_path = :artifact_path_index",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":artifact_path_index", new AttributeValue { S =  artifactPath }}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(queryRequest);
            return response.Items.Select(item => new Artifact
            {
                JobId = item["job_id"].S,
                Index = int.Parse(item["index"].N),
                StepId = item["step_id"].S,
                Name = item["artifact_name"].S,
                StoragePath = item["storage_path"].S
            }).ToList();
        }
    }
}
