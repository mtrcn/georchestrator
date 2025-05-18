using System.Text.Json.Serialization;
using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Activities;
using GEOrchestrator.Domain.Dtos;

namespace GEOrchestrator.Domain
{
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(StepExecutionInput))]
    [JsonSerializable(typeof(StartedReportActivity))]
    [JsonSerializable(typeof(ErrorMessageActivity))]
    [JsonSerializable(typeof(InformationMessageActivity))]
    [JsonSerializable(typeof(CompletedReportActivity))]
    [JsonSerializable(typeof(StepExecutionActivityDto))]
    [JsonSerializable(typeof(SendOutputActivityDto))]
    public partial class GEOrchestratorJsonContext : JsonSerializerContext
    {
    }
} 