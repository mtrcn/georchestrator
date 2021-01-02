using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Executions;

namespace GEOrchestrator.Function.Clients
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

        public async Task<string> SendActivity(string apiUrl, ExecutionStepActivity activity)
        {
            var response = await _policy
                .ExecuteAsync(() => _httpClient.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json")));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Send Activity {activity.ActivityType} - Orchestrator API ({apiUrl}) cannot be called successfully");
                _logger.LogError($"Send Activity {activity.ActivityType} - Orchestrator API Response: {responseContent}");
            }

            return responseContent;
        }
    }
}
