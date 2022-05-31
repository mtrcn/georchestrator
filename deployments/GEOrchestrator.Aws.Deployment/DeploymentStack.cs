using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.ApiGateway;
using Pulumi.Aws.DynamoDB;
using Pulumi.Aws.DynamoDB.Inputs;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ecs;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iam.Inputs;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Aws.S3;
using System.Text.Json;
using Pulumi.Aws.Ec2.Inputs;

namespace GEOrchestrator.Aws.Deployment
{
    public class DeploymentStack : Stack
    {
        public DeploymentStack()
        {
            var applicationConfig = new Pulumi.Config();
            var currentRegion = Output.Create(GetRegion.InvokeAsync());
            var objectRepository = CreateObjectRepository();
            var network = CreateNetworks();
            var tables = CreateTables();
            var orchestrator = CreateContainerOrchestrator();
            var workflowManagerFunctionInvokeArn = CreateWorkflowManager(new WorkflowManagerArguments
            {
                ApiUrl = applicationConfig.Require("api.url"),
                FargateRegion = currentRegion.Apply(t => t.Name),
                TasksTableArn = tables.TasksTableArn,
                JobsTableArn = tables.JobsTableArn,
                ArtifactsTableArn = tables.ArtifactsTableArn,
                WorkflowsTableArn = tables.WorkflowsTableArn,
                ExecutionStepMessagesTableArn = tables.ExecutionStepMessagesTableArn,
                ExecutionStepsTableArn = tables.ExecutionStepsTableArn,
                ParametersTableArn = tables.ParametersTableArn,
                S3BucketName = objectRepository.S3BucketName,
                S3BucketArn = objectRepository.S3BucketArn,
                FargateClusterName = orchestrator.ClusterName,
                FargateClusterArn = orchestrator.ClusterArn,
                FargateSubnetId = network.FargateSubnetId,
                FargateSecurityGroupId = network.SecurityGroupId
            });
            var taskManagerFunctionInvokeArn = CreateTaskManager(tables.TasksTableArn);
            CreateApiGateway(new ApiGatewayArguments
            {
                ApiUrl = applicationConfig.Require("api.url"),
                WorkflowManagerFunctionInvokeArn = workflowManagerFunctionInvokeArn,
                TaskManagerFunctionInvokeArn = taskManagerFunctionInvokeArn
            });
        }

        private void CreateApiGateway(ApiGatewayArguments apiGatewayArguments)
        {
            var api = new RestApi( "georchestrator-api", new RestApiArgs
            {
                Name = "georchestrator-api",
                Description = "GEOrchestrator APIs"
            });
            #region workflows
            var workflowResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-workflow-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "workflows",
                ParentId = api.RootResourceId,
                RestApi = api.Id
            });
            var workflowMethod = new Method("georchestrator-api-workflow-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = workflowResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var workflowIntegration = new Integration("georchestrator-api-workflow-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = workflowResource.Id,
                HttpMethod = workflowMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { workflowMethod } });
            var workflowProxyResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-workflow-proxy-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "{proxy+}",
                ParentId = workflowResource.Id,
                RestApi = api.Id
            });
            var workflowProxyMethod = new Method("georchestrator-api-workflow-proxy-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = workflowProxyResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var workflowProxyIntegration = new Integration("georchestrator-api-workflow-proxy-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = workflowProxyResource.Id,
                HttpMethod = workflowProxyMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { workflowProxyMethod } });
            #endregion

            #region processes
            var processResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-process-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "processes",
                ParentId = api.RootResourceId,
                RestApi = api.Id
            });
            var processMethod = new Method("georchestrator-api-process-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = processResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var processIntegration = new Integration("georchestrator-api-process-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = processResource.Id,
                HttpMethod = processMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { processMethod } });
            var processProxyResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-process-proxy-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "{proxy+}",
                ParentId = processResource.Id,
                RestApi = api.Id
            });
            var processProxyMethod = new Method("georchestrator-api-process-proxy-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = processProxyResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var processProxyIntegration = new Integration("georchestrator-api-process-proxy-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = processProxyResource.Id,
                HttpMethod = processProxyMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { processProxyMethod } });
            #endregion

            #region jobs
            var jobResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-job-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "jobs",
                ParentId = api.RootResourceId,
                RestApi = api.Id
            });
            var jobMethod = new Method("georchestrator-api-job-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = jobResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var jobIntegration = new Integration("georchestrator-api-job-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = jobResource.Id,
                HttpMethod = jobMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { jobMethod } });
            var jobProxyResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-job-proxy-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "{proxy+}",
                ParentId = jobResource.Id,
                RestApi = api.Id
            });
            var jobProxyMethod = new Method("georchestrator-api-job-proxy-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = jobProxyResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var jobProxyIntegration = new Integration("georchestrator-api-job-proxy-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = jobProxyResource.Id,
                HttpMethod = jobProxyMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { jobProxyMethod } });
            #endregion

            #region stepexecutions
            var stepExecutionResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-step-execution-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "stepexecutions",
                ParentId = api.RootResourceId,
                RestApi = api.Id
            });
            var stepExecutionMethod = new Method("georchestrator-api-step-execution-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = stepExecutionResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var stepExecutionIntegration = new Integration("georchestrator-api-step-execution-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = stepExecutionResource.Id,
                HttpMethod = stepExecutionMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { stepExecutionMethod } });
            var stepExecutionProxyResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-step-execution-proxy-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "{proxy+}",
                ParentId = stepExecutionResource.Id,
                RestApi = api.Id
            });
            var stepExecutionProxyMethod = new Method("georchestrator-api-step-execution-proxy-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = stepExecutionProxyResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var stepExecutionProxyIntegration = new Integration("georchestrator-api-step-execution-proxy-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = stepExecutionProxyResource.Id,
                HttpMethod = stepExecutionProxyMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.WorkflowManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { stepExecutionProxyMethod } });
            #endregion

            var workflowPermission = new Permission("georchestrator-api-workflow-permission", new PermissionArgs {
                Action = "lambda:InvokeFunction",
                Function = "georchestrator-workflow-manager",
                Principal = "apigateway.amazonaws.com",
                SourceArn = api.ExecutionArn.Apply(executionArn => $"{executionArn}/*/*/*")
            }, new CustomResourceOptions { DependsOn = { workflowIntegration, workflowProxyIntegration, processIntegration, processProxyIntegration, jobIntegration, jobProxyIntegration, stepExecutionIntegration, stepExecutionProxyIntegration } });
            var taskResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-task-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "tasks",
                ParentId = api.RootResourceId,
                RestApi = api.Id
            });
            var taskProxyResource = new Pulumi.Aws.ApiGateway.Resource("georchestrator-api-task-proxy-resource", new Pulumi.Aws.ApiGateway.ResourceArgs
            {
                PathPart = "{proxy+}",
                ParentId = taskResource.Id,
                RestApi = api.Id
            });
            var taskProxyMethod = new Method("georchestrator-api-task-proxy-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = taskProxyResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var taskProxyIntegration = new Integration("georchestrator-api-task-proxy-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = taskProxyResource.Id,
                HttpMethod = taskProxyMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.TaskManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { taskProxyMethod } });
            var taskMethod = new Method("georchestrator-api-task-method", new MethodArgs
            {
                RestApi = api.Id,
                ResourceId = taskResource.Id,
                HttpMethod = "ANY",
                Authorization = "NONE"
            });
            var taskIntegration = new Integration("georchestrator-api-task-integration", new IntegrationArgs
            {
                RestApi = api.Id,
                ResourceId = taskResource.Id,
                HttpMethod = taskMethod.HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = apiGatewayArguments.TaskManagerFunctionInvokeArn
            }, new CustomResourceOptions { DependsOn = { taskMethod } });
            var taskPermission = new Permission("georchestrator-api-task-permission", new PermissionArgs
            {
                Action = "lambda:InvokeFunction",
                Function = "georchestrator-task-manager",
                Principal = "apigateway.amazonaws.com",
                SourceArn = api.ExecutionArn.Apply(executionArn => $"{executionArn}/*/*/*")
            }, new CustomResourceOptions { DependsOn = { taskIntegration, taskProxyIntegration } });
            var deployment = new Pulumi.Aws.ApiGateway.Deployment("deployment", new DeploymentArgs
            {
                RestApi = api.Id
            }, new CustomResourceOptions { DependsOn = { workflowPermission, taskPermission } });
            var stage = new Stage("stage", new StageArgs
            {
                Deployment = deployment.Id,
                RestApi = api.Id,
                StageName = "stage"
            });
            var _____ = new BasePathMapping("georchestrator-api-base-path-mapping", new BasePathMappingArgs
            {
                RestApi = api.Id,
                DomainName = apiGatewayArguments.ApiUrl,
                StageName = stage.StageName
            });
        }

        private NetworkOutputs CreateNetworks()
        {
            var vpc = new Vpc("georchestrator-vpc", new VpcArgs
            {
                CidrBlock = "10.0.0.0/16"
            });

            var internetGateway = new InternetGateway("georchestrator-igw", new InternetGatewayArgs
            {
                VpcId = vpc.Id
            });

            var routeTable = new RouteTable("georchestrator-route-table", new RouteTableArgs
            {
                VpcId = vpc.Id,
                Routes =
                {
                    new RouteTableRouteArgs
                    {
                        CidrBlock = "0.0.0.0/0",
                        GatewayId = internetGateway.Id,
                    }
                }
            });

            var fargateSubnet = new Subnet("georchestrator-fargate-subnet", new SubnetArgs
            {
                VpcId = vpc.Id,
                CidrBlock = "10.0.1.0/24"
            });

            var routeTableAssociation = new RouteTableAssociation("georchestrator-route-table-association", new RouteTableAssociationArgs
            {
                SubnetId = fargateSubnet.Id,
                RouteTableId = routeTable.Id,
            });

            return new NetworkOutputs {FargateSubnetId = fargateSubnet.Id, SecurityGroupId = vpc.DefaultSecurityGroupId};
        }

        private ContainerOrchestratorOutputs CreateContainerOrchestrator()
        {
            var cluster = new Cluster("georchestrator-cluster", new ClusterArgs
            {
                Name = "georchestrator-cluster"
            });
            
            return new ContainerOrchestratorOutputs
            {
                ClusterName = cluster.Name,
                ClusterArn = cluster.Arn
            };
        }

        private ObjectRepositoryOutputs CreateObjectRepository()
        {
            var bucket = new Bucket("georchestrator-objects", new BucketArgs
            {
                BucketName = "georchestrator-objects"
            });
            return new ObjectRepositoryOutputs
            {
                S3BucketArn = bucket.Arn,
                S3BucketName = bucket.BucketName
            };
        }

        private TableOutputs CreateTables()
        {
            var tasksTable = new Table("georchestrator-tasks", new TableArgs
            {
                Name = "georchestrator-tasks",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "task_name",
                        Type = "S",
                    }
                },
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "task_name"
            });

            var jobsTable = new Table("georchestrator-jobs", new TableArgs
            {
                Name = "georchestrator-jobs",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "id",
                        Type = "S",
                    }
                },
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "id"
            });

            var artifactsTable = new Table("georchestrator-artifacts", new TableArgs
            {
                Name = "georchestrator-artifacts",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "id",
                        Type = "S",
                    },
                    new TableAttributeArgs
                    {
                        Name = "artifact_path",
                        Type = "S",
                    }
                },
                GlobalSecondaryIndexes =
                {
                    new TableGlobalSecondaryIndexArgs
                    {
                        Name = "artifact_path_index",
                        HashKey = "artifact_path",
                        ProjectionType = "ALL"
                    }
                },
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "id"
            });

            var executionMessagesTable = new Table("georchestrator-execution-messages", new TableArgs
            {
                Name = "georchestrator-execution-messages",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "id",
                        Type = "S",
                    },
                    new TableAttributeArgs
                    {
                        Name = "sent_on",
                        Type = "N",
                    }
                },
                RangeKey = "sent_on",
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "id"
            });

            var parametersTable = new Table("georchestrator-parameters", new TableArgs
            {
                Name = "georchestrator-parameters",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "id",
                        Type = "S",
                    },
                    new TableAttributeArgs
                    {
                        Name = "parameter_path",
                        Type = "S",
                    }
                },
                GlobalSecondaryIndexes =
                {
                    new TableGlobalSecondaryIndexArgs
                    {
                        Name = "parameter_path_index",
                        HashKey = "parameter_path",
                        ProjectionType = "ALL"
                    }
                },
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "id"
            });

            var stepExecutionsTable = new Table("georchestrator-step-executions", new TableArgs
            {
                Name = "georchestrator-step-executions",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "id",
                        Type = "S",
                    },
                    new TableAttributeArgs
                    {
                        Name = "parent_step_execution_id",
                        Type = "S",
                    }
                },
                GlobalSecondaryIndexes =
                {
                    new TableGlobalSecondaryIndexArgs
                    {
                        Name = "parent_step_execution_id_index",
                        HashKey = "parent_step_execution_id",
                        ProjectionType = "ALL"
                    }
                },
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "id"
            });

            var workflowsTable = new Table("georchestrator-workflows", new TableArgs
            {
                Name = "georchestrator-workflows",
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Name = "workflow_name",
                        Type = "S",
                    }
                },
                BillingMode = "PAY_PER_REQUEST",
                HashKey = "workflow_name"
            });

            return new TableOutputs
            {
                WorkflowsTableArn = workflowsTable.Arn,
                JobsTableArn = jobsTable.Arn,
                TasksTableArn = tasksTable.Arn,
                ArtifactsTableArn = artifactsTable.Arn,
                ParametersTableArn = parametersTable.Arn,
                ExecutionStepsTableArn = stepExecutionsTable.Arn,
                ExecutionStepMessagesTableArn = executionMessagesTable.Arn
            };
        }

        private Output<string> CreateWorkflowManager(WorkflowManagerArguments arguments)
        {
            var roleFargateExecution = new Role("georchestrator-fargate-execution-role", new RoleArgs
            {
                Name = "georchestrator-fargate-execution-role",
                AssumeRolePolicy = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    { "Version", "2012-10-17" },
                    { "Statement", new[]
                        {
                            new Dictionary<string, object?>
                            {
                                { "Action", "sts:AssumeRole" },
                                { "Effect", "Allow" },
                                { "Sid", "" },
                                { "Principal", new Dictionary<string, object?>
                                {
                                    { "Service", "ecs-tasks.amazonaws.com" },
                                } }
                            }
                        }
                    }
                }),
                InlinePolicies =
                {
                    new RoleInlinePolicyArgs
                    {
                        Name = "fargate",
                        Policy =JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            { "Version", "2012-10-17" },
                            { "Statement", new[]
                                {
                                    new Dictionary<string, object?>
                                    {
                                        { "Action", new[]
                                            {
                                                "logs:CreateLogStream",
                                                "logs:PutLogEvents"
                                            }
                                        },
                                        { "Effect", "Allow" },
                                        { "Resource", "arn:aws:logs:*" }
                                    }
                                }
                            }
                        })
                    }
                }
            });

            var roleWorkflowManagerFunction = new Role("georchestrator-workflow-manager-role", new RoleArgs
            {
                Name = "georchestrator-workflow-manager-role",
                AssumeRolePolicy = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    { "Version", "2012-10-17" },
                    { "Statement", new[]
                        {
                            new Dictionary<string, object?>
                            {
                                { "Action", "sts:AssumeRole" },
                                { "Effect", "Allow" },
                                { "Sid", "" },
                                { "Principal", new Dictionary<string, object?>
                                {
                                    { "Service", "lambda.amazonaws.com" },
                                } }
                            }
                        }
                    }
                }),
                InlinePolicies =
                {
                    new RoleInlinePolicyArgs
                    {
                        Name = "dynamodb",
                        Policy = Output.Tuple(arguments.WorkflowsTableArn, arguments.TasksTableArn, arguments.JobsTableArn, arguments.ArtifactsTableArn, arguments.ParametersTableArn, arguments.ExecutionStepsTableArn, arguments.ExecutionStepMessagesTableArn).Apply(args =>
                        {
                            var (workflowsTableArn, tasksTableArn, jobsTableArn, artifactsTableArn, parametersTableArn, executionStepsTableArn, executionStepMessagesTableArn) = args;

                            return JsonSerializer.Serialize(new Dictionary<string, object?>
                            {
                                {"Version", "2012-10-17"},
                                {
                                    "Statement", new[]
                                    {
                                        new Dictionary<string, object?>
                                        {
                                            {
                                                "Action",
                                                new[]
                                                {
                                                    "dynamodb:PutItem", "dynamodb:Query", "dynamodb:Scan", "dynamodb:GetItem", "dynamodb:UpdateItem"
                                                }
                                            },
                                            {"Effect", "Allow"},
                                            {
                                                "Resource", new[]
                                                {
                                                    workflowsTableArn,
                                                    jobsTableArn,
                                                    tasksTableArn,
                                                    artifactsTableArn,
                                                    $"{artifactsTableArn}/index/*",
                                                    parametersTableArn,
                                                    $"{parametersTableArn}/index/*",
                                                    executionStepsTableArn,
                                                    $"{executionStepsTableArn}/index/*",
                                                    executionStepMessagesTableArn
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                        })
                    },
                    new RoleInlinePolicyArgs
                    {
                        Name = "bucket",
                        Policy = arguments.S3BucketArn.Apply(s3BucketArn => JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            { "Version", "2012-10-17" },
                            { "Statement", new[]
                                {
                                    new Dictionary<string, object?>
                                    {
                                        { "Action", new[]
                                            {
                                                "s3:*"
                                            }
                                        },
                                        { "Effect", "Allow" },
                                        { "Resource", $"{s3BucketArn}/*" }
                                    }
                                }
                            }
                        }))
                    },
                    new RoleInlinePolicyArgs
                    {
                        Name = "ecs",
                        Policy = JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            { "Version", "2012-10-17" },
                            { "Statement", new[]
                                {
                                    new Dictionary<string, object?>
                                    {
                                        { "Action", new[]
                                            {
                                                "ecs:RegisterTaskDefinition",
                                                "ecs:DeregisterTaskDefinition",
                                                "ecs:RunTask"
                                            }
                                        },
                                        { "Effect", "Allow" },
                                        { "Resource", "*" }
                                    }
                                }
                            }
                        })
                    },
                    new RoleInlinePolicyArgs
                    {
                        Name = "fargate",
                        Policy = roleFargateExecution.Arn.Apply(roleFargateExecutionArn => JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            { "Version", "2012-10-17" },
                            { "Statement", new[]
                                {
                                    new Dictionary<string, object?>
                                    {
                                        { "Action", new[]
                                            {
                                                "iam:PassRole"
                                            }
                                        },
                                        { "Effect", "Allow" },
                                        { "Resource", roleFargateExecutionArn }
                                    }
                                }
                            }
                        }))
                    },
                    new RoleInlinePolicyArgs
                    {
                        Name = "logs",
                        Policy = JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            { "Version", "2012-10-17" },
                            { "Statement", new[]
                                {
                                    new Dictionary<string, object?>
                                    {
                                        { "Action", new[]
                                            {
                                                "logs:CreateLogGroup",
                                                "logs:CreateLogStream",
                                                "logs:PutLogEvents"
                                            }
                                        },
                                        { "Effect", "Allow" },
                                        { "Resource", "*" }
                                    }
                                }
                            }
                        })
                    }
                }
            });


            var workflowManagerFunction = new Function("georchestrator-workflow-manager", new FunctionArgs
            {
                Name = "georchestrator-workflow-manager",
                Timeout = 60,
                Runtime = "dotnetcore3.1",
                Code = new FileArchive("../release/workflow-manager-release/release.zip"),
                Handler = "GEOrchestrator.WorkflowManager::GEOrchestrator.WorkflowManager.LambdaEntryPoint::FunctionHandlerAsync",
                Environment = new FunctionEnvironmentArgs
                {
                    Variables = new InputMap<string>
                    {
                        {"OBJECT_REPOSITORY_PROVIDER", "s3"},
                        {"AWS_S3_BUCKET_NAME", arguments.S3BucketName},
                        {"JOB_REPOSITORY_PROVIDER", "dynamodb"},
                        {"PARAMETER_REPOSITORY_PROVIDER", "dynamodb"},
                        {"ARTIFACT_REPOSITORY_PROVIDER", "dynamodb"},
                        {"WORKFLOW_REPOSITORY_PROVIDER", "dynamodb"},
                        {"TASK_REPOSITORY_PROVIDER", "dynamodb"},
                        {"EXECUTION_STEP_REPOSITORY_PROVIDER", "dynamodb"},
                        {"EXECUTION_STEP_MESSAGE_REPOSITORY_PROVIDER", "dynamodb"},
                        {"WORKFLOW_API_URL", $"https://{arguments.ApiUrl}" },
                        {"CONTAINER_PROVIDER", "fargate"},
                        {"FARGATE_REGION", arguments.FargateRegion},
                        {"FARGATE_EXECUTION_ROLE_ARN", roleFargateExecution.Arn },
                        {"FARGATE_CLUSTER_NAME", arguments.FargateClusterName },
                        {"FARGATE_SUBNET_ID", arguments.FargateSubnetId },
                        {"FARGATE_SECURITY_GROUP_ID", arguments.FargateSecurityGroupId}
                    }
                },
                Role = roleWorkflowManagerFunction.Arn
            });

            return workflowManagerFunction.InvokeArn;
        }

        private Output<string> CreateTaskManager(Output<string> tasksTableArn)
        {
            var roleTaskManagerFunction = new Role("georchestrator-task-manager-role", new RoleArgs
            {
                Name = "georchestrator-task-manager-role",
                AssumeRolePolicy = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    { "Version", "2012-10-17" },
                    { "Statement", new[]
                        {
                            new Dictionary<string, object?>
                            {
                                { "Action", "sts:AssumeRole" },
                                { "Effect", "Allow" },
                                { "Sid", "" },
                                { "Principal", new Dictionary<string, object?>
                                {
                                    { "Service", "lambda.amazonaws.com" },
                                } }
                            }
                        }
                    }
                }),
                InlinePolicies =
                {
                    new RoleInlinePolicyArgs
                    {
                        Name = "dynamodb",
                        Policy = tasksTableArn.Apply(tableArn =>
                        {
                            return JsonSerializer.Serialize(new Dictionary<string, object?>
                            {
                                {"Version", "2012-10-17"},
                                {
                                    "Statement", new[]
                                    {
                                        new Dictionary<string, object?>
                                        {
                                            {
                                                "Action",
                                                new[]
                                                {
                                                    "dynamodb:PutItem", "dynamodb:Query", "dynamodb:Scan",
                                                    "dynamodb:GetItem"
                                                }
                                            },
                                            {"Effect", "Allow"},
                                            {
                                                "Resource", new[]
                                                {
                                                    tableArn
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                        })
                    },
                    new RoleInlinePolicyArgs
                    {
                        Name = "logs",
                        Policy = JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            { "Version", "2012-10-17" },
                            { "Statement", new[]
                                {
                                    new Dictionary<string, object?>
                                    {
                                        { "Action", new[]
                                            {
                                                "logs:CreateLogGroup",
                                                "logs:CreateLogStream",
                                                "logs:PutLogEvents"
                                            }
                                        },
                                        { "Effect", "Allow" },
                                        { "Resource", "*" }
                                    }
                                }
                            }
                        })
                    }
                }
            });

            var taskManagerFunction = new Function("georchestrator-task-manager", new FunctionArgs
            {
                Name = "georchestrator-task-manager",
                Timeout = 60,
                Runtime = "dotnetcore3.1",
                Code = new FileArchive("../release/task-manager-release/release.zip"),
                Handler = "GEOrchestrator.TaskManager::GEOrchestrator.TaskManager.LambdaEntryPoint::FunctionHandlerAsync",
                Environment = new FunctionEnvironmentArgs
                {
                    Variables = new InputMap<string>
                    {
                        { "TASK_REPOSITORY_PROVIDER", "dynamodb" }
                    }
                },
                Role = roleTaskManagerFunction.Arn
            });

            return taskManagerFunction.InvokeArn;
        }
    }
}