using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using System.Collections.Generic;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.S3;

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

            var jobsTable = new Table(this, "GEOrchestratorJobsTable", new TableProps
            {
                TableName = "georchestrator-jobs",
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            var artifactsTable = new Table(this, "GEOrchestratorArtifactsTable", new TableProps
            {
                TableName = "georchestrator-artifacts",
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            artifactsTable.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
            {
                IndexName = "artifact_path_index",
                PartitionKey = new Attribute
                {
                    Name = "artifact_path",
                    Type = AttributeType.STRING
                },
                ProjectionType = ProjectionType.ALL
            });

            var executionMessagesTable = new Table(this, "GEOrchestratorExecutionMessagesTable", new TableProps
            {
                TableName = "georchestrator-execution-messages",
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                SortKey = new Attribute
                {
                    Name = "sent_on",
                    Type = AttributeType.NUMBER
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            var parametersTable = new Table(this, "GEOrchestratorParametersTable", new TableProps
            {
                TableName = "georchestrator-parameters",
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            parametersTable.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
            {
                IndexName = "parameter_path_index",
                PartitionKey = new Attribute
                {
                    Name = "parameter_path",
                    Type = AttributeType.STRING
                },
                ProjectionType = ProjectionType.ALL
            });

            var stepExecutionsTable = new Table(this, "GEOrchestratorStepExecutionsTable", new TableProps
            {
                TableName = "georchestrator-step-executions",
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            stepExecutionsTable.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
            {
                IndexName = "parent_step_execution_id_index",
                PartitionKey = new Attribute
                {
                    Name = "parent_step_execution_id ",
                    Type = AttributeType.STRING
                },
                ProjectionType = ProjectionType.ALL
            });

            var workflowsTable = new Table(this, "GEOrchestratorWorkflowsTable", new TableProps
            {
                TableName = "georchestrator-workflows",
                PartitionKey = new Attribute
                {
                    Name = "workflow_name",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            var bucket = new Bucket(this, "GEOrchestratorWorkflowOutputsStorage", new BucketProps
            {
                BucketName = "georchestrator-objects"
            });

            var cluster = new Cluster(this, "GEOrchestratorContainerService", new ClusterProps
            {
                ClusterName = "georchestrator"
            });

            var roleFargetExecution = new Role(this, "GEOrchestratorFargateExecutionRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
                InlinePolicies = new Dictionary<string, PolicyDocument>
                {
                    {
                        "fargate",
                        new PolicyDocument(new PolicyDocumentProps
                        {
                            Statements = new PolicyStatement[]{
                                new PolicyStatement(new PolicyStatementProps
                                {
                                    Actions = new string[]{"ecr:GetAuthorizationToken", "ecr:BatchCheckLayerAvailability", "ecr:GetDownloadUrlForLayer", "ecr:BatchGetImage", "logs:CreateLogStream", "logs:PutLogEvents"},
                                    Resources = new string[]{ jobsTable.TableArn, artifactsTable.TableArn, executionMessagesTable.TableArn, parametersTable.TableArn, stepExecutionsTable.TableArn, workflowsTable.TableArn }
                                })
                            }
                        })
                    }
                }
            });

            var roleWorkflowManagerFunction = new Role(this, "GEOrchestratorWorkflowManagerExecutionRole", new RoleProps
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
                                    Actions = new string[]{"dynamodb:PutItem", "dynamodb:Query", "dynamodb:Scan", "dynamodb:GetItem"},
                                    Resources = new string[]{ jobsTable.TableArn, artifactsTable.TableArn, executionMessagesTable.TableArn, parametersTable.TableArn, stepExecutionsTable.TableArn, workflowsTable.TableArn }
                                })
                            }
                        })
                    },
                    {
                        "bucket",
                        new PolicyDocument(new PolicyDocumentProps
                        {
                            Statements = new PolicyStatement[]{
                                new PolicyStatement(new PolicyStatementProps
                                {
                                    Actions = new string[]{"s3:*"},
                                    Resources = new string[]{ bucket.BucketArn }
                                })
                            }
                        })
                    }
                }
            });

            var workflowManagerFunction = new Function(this, "GEOrchestratorWorkflowManager", new FunctionProps
            {
                FunctionName = "GEOrchestratorTaskManager",
                Timeout = Duration.Minutes(1),
                Runtime = Runtime.DOTNET_CORE_3_1,
                Code = Code.FromAsset("../GEOrchestrator.WorkflowManager/bin/Release/netcoreapp3.1/publish/"),
                Handler = "GEOrchestrator.TaskManager::GEOrchestrator.WorkflowManager.LambdaEntryPoint::FunctionHandlerAsync",
                Environment = new Dictionary<string, string>{
                    {"TASK_REPOSITORY_PROVIDER", "dynamodb" },
                    {"OBJECT_REPOSITORY_PROVIDER", "s3"},
                    {"AWS_S3_BUCKET_NAME", bucket.BucketName},
                    {"JOB_REPOSITORY_PROVIDER", "dynamodb"},
                    {"PARAMETER_REPOSITORY_PROVIDER", "dynamodb"},
                    {"ARTIFACT_REPOSITORY_PROVIDER", "dynamodb"},
                    {"WORKFLOW_REPOSITORY_PROVIDER", "dynamodb"},
                    {"TASK_REPOSITORY_PROVIDER", "dynamodb"},
                    {"EXECUTION_STEP_REPOSITORY_PROVIDER", "dynamodb"},
                    {"EXECUTION_STEP_MESSAGE_REPOSITORY_PROVIDER", "dynamodb"},
                    {"WORKFLOW_API_URL", "https://api.georchestrator.app"},
                    {"CONTAINER_PROVIDER", "fargate"},
                    {"FARGATE_REGION", "eu-west-1"},
                    {"FARGATE_EXECUTION_ROLE_ARN", roleFargetExecution.RoleArn},
                    {"FARGATE_CLUSTER_NAME", cluster.ClusterName},
                    {"FARGATE_SUBNET_ID", "subnet-e83c1e8c"},
                    {"FARGATE_SECURITY_GROUP_ID", "sg-18c2ab7e"}
                },
                Role = roleWorkflowManagerFunction
            });

            var workflowManagerIntegration = new LambdaIntegration(workflowManagerFunction);
            var workflowResource = api.Root.AddResource("/");
            workflowResource.AddMethod("ANY", workflowManagerIntegration);
        }
    }
}
