using GEOrchestrator.Domain;
using GEOrchestrator.Domain.Dtos;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GEOrchestrator.ContainerAgent.Clients
{
    public class OrchestratorClient : IOrchestratorClient
    {
        private readonly ILogger<OrchestratorClient> _logger;
        private readonly HttpStatusCode[] _httpStatusCodesWorthRetrying = {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;
        private readonly HttpClient _httpClient;

        public OrchestratorClient(ILogger<OrchestratorClient> logger)
        {
            _logger = logger;
            _policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => _httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5)); //wait: 30secs total
            _httpClient = new HttpClient();
        }

        public async Task<string> SendActivityAsync(string apiUrl, StepExecutionActivityDto activity)
        {
            var response = await _policy
                .ExecuteAsync(() => _httpClient.PostAsync(apiUrl, new StringContent(JsonSerializer.Serialize(activity, GEOrchestratorJsonContext.Default.StepExecutionActivityDto), Encoding.UTF8, "application/json")));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Send Activity {activity.Type} - Orchestrator API ({apiUrl}) cannot be called successfully");
                _logger.LogError($"Send Activity {activity.Type} - Orchestrator API Response: {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> SendOutputAsync(string apiUrl, SendOutputActivityDto activity)
        {
            var response = await _policy
                .ExecuteAsync(() => _httpClient.PostAsync(apiUrl, new StringContent(JsonSerializer.Serialize(activity, GEOrchestratorJsonContext.Default.SendOutputActivityDto), Encoding.UTF8, "application/json")));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Send Output - Workflow API ({apiUrl}) cannot be called successfully");
                _logger.LogError($"Send Output - Workflow API Response: {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> ReceiveInputsAsync(string apiUrl)
        {
            var response = await _policy
                .ExecuteAsync(() => _httpClient.GetAsync(apiUrl));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Receive Inputs - Workflow API ({apiUrl}) cannot be called successfully");
                _logger.LogError($"Receive Inputs - Workflow API Response: {responseContent}");
            }

            return responseContent;
        }
    }
}
