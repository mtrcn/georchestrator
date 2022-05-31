using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GEOrchestrator.ContainerAgent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GEOrchestrator.ContainerAgent
{
    public class FunctionCaller : IFunctionCaller
    {
        private readonly IOrchestratorService _orchestratorService;
        private readonly ILogger<FunctionCaller> _logger;
        private readonly string _command;

        public FunctionCaller(IConfiguration configuration, IOrchestratorService orchestratorService, ILogger<FunctionCaller> logger)
        {
            _orchestratorService = orchestratorService;
            _logger = logger;
            _command = configuration["FUNCTION_COMMAND"];
        }

        public async Task RunAsync()
        {
            if (string.IsNullOrEmpty(_command))
            {
                _logger.LogError("Function command is not found!.");
                await _orchestratorService.SendErrorMessageAsync("Function command is not found!");
                return;
            }

            _orchestratorService.CreateLockFile();
            _orchestratorService.CreateDirectories();

            await _orchestratorService.ReceiveInputsAsync();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(GetShell(), GetCommand(_command))
                {
                    UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true
                }
            };
            process.Start();

            _logger.LogInformation("Function execution started.");
            await _orchestratorService.ReportStartAsync(DateTime.UtcNow);

            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                _logger.LogError(line);
                _orchestratorService.SendInformationMessageAsync(line).Wait();
            }

            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                _logger.LogInformation(line);
                _orchestratorService.SendErrorMessageAsync(line).Wait();
            }
            process.WaitForExit();
            _logger.LogInformation("Function execution finished.");

            await _orchestratorService.SendOutputParametersAsync();
            await _orchestratorService.SendOutputArtifactsAsync();
            await _orchestratorService.ReportCompletedAsync(DateTime.UtcNow);
            _orchestratorService.RemoveLockFile();
        }

        private static string GetShell()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";
        }

        private static string GetCommand(string command)
        {
            var escapedCommand = command.Replace("\"", "\\\"");

            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c \"{escapedCommand}\"" : $"-c \"{escapedCommand}\"";
        }
    }
}
