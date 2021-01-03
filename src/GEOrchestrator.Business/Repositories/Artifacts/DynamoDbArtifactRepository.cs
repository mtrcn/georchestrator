using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GEOrchestrator.Domain.Models.Artifacts;

namespace GEOrchestrator.Business.Repositories.Artifacts
{
    public class DynamoDbArtifactRepository : IArtifactRepository
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private const string TableName = "georchestrator-artifacts";

        public DynamoDbArtifactRepository(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task<(string marker, Artifact artifact)> GetNextAsync(string workflowRunId, string stepId, string name, string lastMarker = null)
        {
            var queryRequest = new QueryRequest
            {
                TableName = TableName,
                IndexName = "reference_id_index",
                Limit = 1,
                KeyConditionExpression = "reference_id=:reference_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    { ":reference_id", new AttributeValue ($"{{{{{workflowRunId}.{stepId}.{name}}}}}") }
                }
            };

            if (!string.IsNullOrEmpty(lastMarker))
            {
                queryRequest.ExclusiveStartKey.Add("workflow_run_id", new AttributeValue(workflowRunId));
                queryRequest.ExclusiveStartKey.Add("storage_path", new AttributeValue(lastMarker));
                queryRequest.ExclusiveStartKey.Add("reference_id", new AttributeValue($"{{{{{workflowRunId}.{stepId}.{name}}}}}"));
            }

            var response = await _amazonDynamoDb.QueryAsync(queryRequest);

            if (response.Count == 0)
                return (null, null);

            return (
                response.LastEvaluatedKey.Count == 0 ? string.Empty : response.LastEvaluatedKey["storage_path"].S,
                new Artifact
                {
                    WorkflowRunId = response.Items.FirstOrDefault()?["workflow_run_id"].S,
                    RelativePath = response.Items.FirstOrDefault()?["relative_path"].S,
                    Name = response.Items.FirstOrDefault()?["artifact_name"].S,
                    StoragePath = response.Items.FirstOrDefault()?["storage_path"].S,
                    StepId = response.Items.FirstOrDefault()?["step_id"].S
                }
            );
        }

        public async Task<List<Artifact>> GetArtifactsByStepIdAndName(string workflowRunId, string stepId, string name)
        {
            Dictionary<string, AttributeValue> lastEvaluatedKey = null;
            var result = new List<Artifact>();

            do
            {
                var queryRequest = new QueryRequest
                {
                    TableName = TableName,
                    IndexName = "reference_id_index",
                    Limit = 1,
                    ExclusiveStartKey = lastEvaluatedKey,
                    KeyConditionExpression = "reference_id=:reference_id",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        { ":reference_id", new AttributeValue ($"{{{{{workflowRunId}.{stepId}.{name}}}}}") }
                    }
                };
                var response = await _amazonDynamoDb.QueryAsync(queryRequest);

                lastEvaluatedKey = response.LastEvaluatedKey;
                result.Add(new Artifact
                {
                    WorkflowRunId = response.Items.FirstOrDefault()?["workflow_run_id"].S,
                    RelativePath = response.Items.FirstOrDefault()?["relative_path"].S,
                    Name = response.Items.FirstOrDefault()?["artifact_name"].S,
                    StoragePath = response.Items.FirstOrDefault()?["storage_path"].S,
                    StepId = response.Items.FirstOrDefault()?["step_id"].S
                });

            } while (lastEvaluatedKey != null && lastEvaluatedKey.Count != 0);

            return result;
        }

        public async Task AddAsync(Artifact artifact)
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"workflow_run_id", new AttributeValue(artifact.WorkflowRunId) },
                    {"reference_id", new AttributeValue($"{{{{{artifact.WorkflowRunId}.{artifact.StepId}.{artifact.Name}}}}}") },
                    {"step_id", new AttributeValue(artifact.StepId) },
                    {"artifact_name", new AttributeValue(artifact.Name) },
                    {"relative_path", new AttributeValue(artifact.RelativePath) },
                    {"storage_path", new AttributeValue(artifact.StoragePath) }
                }
            };
            await _amazonDynamoDb.PutItemAsync(putItemRequest);
        }
    }
}
