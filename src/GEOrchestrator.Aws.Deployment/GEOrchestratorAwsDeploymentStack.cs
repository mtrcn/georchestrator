using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using System.Collections.Generic;

namespace GEOrchestrator.Aws.Deployment
{
    public class GEOrchestratorAwsDeploymentStack : Stack
    {
        internal GEOrchestratorAwsDeploymentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var tasksTable = new Table(this, "GEOrchestratorTasksTable", new TableProps
            {
                TableName = "georchestrator-tasks",
                PartitionKey = new Attribute
                {
                    Name = "task_name",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });


            var roleTaskManagerFunction = new Role(this, "GEOrchestratorTaskManagerExecutionRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                InlinePolicies = new Dictionary<string, PolicyDocument>
                {
                    {
                        "dynamodb",
                        new PolicyDocument(new PolicyDocumentProps
                        {
                            Statements = new PolicyStatement[]{
                                new PolicyStatement(new PolicyStatementProps
                                {
                                    Actions = new string[]{"dynamodb:PutItem"},
                                    Resources = new string[]{ tasksTable.TableArn }
                                })
                            }
                        })
                    }
                }
            });

            var taskManagerFunction = new Function(this, "GEOrchestratorTaskManager", new FunctionProps
            {
                FunctionName = "GEOrchestratorTaskManager",
                Timeout = Duration.Minutes(1),
                Runtime = Runtime.DOTNET_CORE_3_1,
                Code = Code.FromAsset("../GEOrchestrator.TaskManager/bin/Release/netcoreapp3.1/publish/"),
                Handler = "GEOrchestrator.TaskManager::GEOrchestrator.TaskManager.LambdaEntryPoint::FunctionHandlerAsync",
                Environment = new Dictionary<string, string>{
                    { "TASK_REPOSITORY_PROVIDER", "dynamodb" }
                },
                Role = roleTaskManagerFunction
            });


            var api = new RestApi(this, "GEOrchestrator", new RestApiProps
            {
                RestApiName = "GEOrchestrator",
                Description = "GEOrchestrator APIs"
            });

            var taskManagerIntegration = new LambdaIntegration(taskManagerFunction);
            var tasksResource = api.Root.AddResource("tasks");
            tasksResource.AddMethod("ANY", taskManagerIntegration);
        }
    }
}
