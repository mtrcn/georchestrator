using Pulumi;

namespace GEOrchestrator.Aws.Deployment
{
    public class ApiGatewayArguments
    {
        public string ApiUrl { get; set; } 
        public Output<string> ApiFunctionInvokeArn { get; set; }
    }
}
