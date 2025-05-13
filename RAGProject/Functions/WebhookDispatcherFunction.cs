using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spt.Rag.Shared.Utils;
using StackExchange.Redis;

namespace SptRAG.Functions
{
    public class WebhookDispatcherFunction
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;
        private static readonly HttpClient _httpClient = new();

        public WebhookDispatcherFunction(ILoggerFactory loggerFactory, IConnectionMultiplexer redis)
        {
            _logger = loggerFactory.CreateLogger<WebhookDispatcherFunction>();
            _redis = redis.GetDatabase();
        }

        [Function("DispatchWebhookEvent")]
        public async Task RunAsync([ServiceBusTrigger("webhook-events", Connection = "ServiceBusConnection")] string eventJson)
        {
            var webhookEvent = JsonConvert.DeserializeObject<WebhookEventPayload>(eventJson);
            string retryKey = $"webhook:retry:{webhookEvent.EventType}:{webhookEvent.DocumentId}";

            try
            {
                var payload = JsonConvert.SerializeObject(webhookEvent.Payload);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookEvent.TargetUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed with status {response.StatusCode}");
                }

                await _redis.HashDeleteAsync("webhook:retry-metadata", retryKey);
                _logger.LogInformation($"Successfully delivered webhook: {webhookEvent.EventType} for {webhookEvent.DocumentId}");
            }
            catch (Exception ex)
            {
                int retryCount = (int?)await _redis.HashGetAsync("webhook:retry-metadata", retryKey + ":count") ?? 0;
                retryCount++;
                await _redis.HashSetAsync("webhook:retry-metadata", new[]
                {
                    new HashEntry(retryKey + ":payload", eventJson),
                    new HashEntry(retryKey + ":count", retryCount),
                    new HashEntry(retryKey + ":lastError", ex.Message),
                    new HashEntry(retryKey + ":lastAttempt", DateTime.UtcNow.ToString("o"))
                });

                _logger.LogWarning($"Retry #{retryCount} queued for webhook {webhookEvent.EventType} targeting {webhookEvent.TargetUrl}");

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                await RedisQueueHelper.ScheduleRetry("webhook-retry-queue", eventJson, delay);
            }
        }
    }
}
