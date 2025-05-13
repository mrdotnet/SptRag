using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spt.Rag.Shared.Utils;

namespace SptRAG.Functions.Helpers
{
    public static class WebhookHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task NotifyStageProgress(DocumentStageEvent evt, ILogger logger)
        {
            var payload = JsonConvert.SerializeObject(evt);
            var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("StageProgressWebhook"))
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var success = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed webhook with status: {response.StatusCode}");
                }
                return true;
            }, 3, TimeSpan.FromSeconds(2), logger);

            if (!success)
            {
                logger.LogWarning($"Failed to deliver stage progress for {evt.FileName}. Enqueuing for retry.");
                await RedisQueueHelper.PushToRetryQueue("webhook-retry", new
                {
                    evt.FileName,
                    evt.Hash,
                    evt.Stage,
                    evt.Timestamp,
                    WebhookType = "stage-progress"
                });
            }
        }

        public static async Task NotifyFailure(string category, string fileName, string hash, string reason)
        {
            var uri = Environment.GetEnvironmentVariable("FailureWebhook");
            if (string.IsNullOrEmpty(uri)) return;

            var payload = new
            {
                Event = "document.processing.failed",
                Category = category,
                FileName = fileName,
                Hash = hash,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };

            await _httpClient.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
        }
    }

    public class DocumentStageEvent
    {
        public string FileName { get; set; }
        public object Hash { get; set; }
        public object Stage { get; set; }
        public object Timestamp { get; set; }
    }

    public class RedisQueueHelper
    {
        public static async Task PushToRetryQueue(string webhookRetry, object o)
        {
            throw new NotImplementedException();
        }
    }

    public class RetryHelper
    {
        public static async Task<bool> ExecuteWithRetryAsync(Func<bool> func, int i, TimeSpan fromSeconds, ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
