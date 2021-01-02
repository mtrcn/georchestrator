using System.Threading.Tasks;
using GEOrchestrator.Function.Clients;
using GEOrchestrator.Function.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GEOrchestrator.Function
{
    class Program
    {
        static async Task Main()
        {
            var configBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
            var configuration = configBuilder.Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder
                    .AddConsole()
                    .AddFilter(level => level >= LogLevel.Information)
                )
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IOrchestratorService, OrchestratorService>()
                .AddSingleton<IFunctionCaller, FunctionCaller>()
                .AddSingleton<IOrchestratorClient, OrchestratorClient>()
                .AddSingleton<IHttpFileClient, HttpFileClient>()
                .AddSingleton<IFunctionCaller, FunctionCaller>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogInformation("Function caller execution is started.");

            var caller = serviceProvider.GetService<IFunctionCaller>();
            await caller.RunAsync();

            logger.LogInformation("Function caller execution is completed.");
        }
    }
}
