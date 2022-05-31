using GEOrchestrator.Aws.Deployment;
using Pulumi;

await Deployment.RunAsync<DeploymentStack>();
