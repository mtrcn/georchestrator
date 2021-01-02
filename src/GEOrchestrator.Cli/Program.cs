using System;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;

namespace GEOrchestrator.Cli
{
    class Program: ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            // target T as ConsoleAppBase.
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync(args);
        }
    }

    public class Configuration : ConsoleAppBase
    {
        public void Update(string msg)
        {
            //Todo: Update configuration with Docker Registry params and orchestrator api config
            Console.WriteLine(msg);
        }
    }

    public class Function : ConsoleAppBase
    {
        public void Deploy(string msg)
        {
            //Todo: Build docker Image based on Function image
            //Todo: Push docker image to docker registry
            //Todo: Add service to orchestrator 
            Console.WriteLine(msg);
        }
    }
}
