using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace GEOrchestrator.ContainerAgent.Clients
{
    public class HttpFileClient : IHttpFileClient
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

        public HttpFileClient(ILogger<OrchestratorClient> logger)
        {
            _logger = logger;
            _policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => _httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5)); //wait: 30secs total
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromHours(1)
            };
        }

        public async Task DownloadFile(string url, string filePath)
        {
            var response = await _policy.ExecuteAsync(() => _httpClient.GetAsync(url));

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Download File - Orchestrator API cannot be called successfully");
                _logger.LogError($"Download File - Orchestrator API Response Code: {response.StatusCode}");
            }
        }

        public async Task UploadFile(string url, Stream stream)
        {
            var response = await _policy.ExecuteAsync(() => _httpClient.PutAsync(url, new StreamContent(stream)));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Download File - Orchestrator API cannot be called successfully");
                _logger.LogError($"Download File - Orchestrator API Response Code: {response.StatusCode}");
            }
        }
    }
}
