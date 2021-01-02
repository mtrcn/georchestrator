﻿using System;
using System.Threading.Tasks;

namespace GEOrchestrator.Function.Services
{
    public interface IOrchestratorService
    {
        Task ReportStartAsync(DateTime startTime);
        Task SendErrorMessageAsync(string errorMessage);
        Task SendInformationMessageAsync(string infoMessage);
        Task ReportCompletedAsync(DateTime completedTime);
        Task SendOutputParametersAsync();
        Task SendOutputArtifactsAsync();
        Task ReceiveParametersAsync();
        Task ReceiveArtifactsAsync();
        void CreateDirectories();
        void CreateLockFile();
        void RemoveLockFile();
    }
}