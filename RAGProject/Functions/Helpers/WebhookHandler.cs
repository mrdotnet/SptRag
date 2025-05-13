using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spt.Rag.Shared.Utils;

namespace SptRAG.Functions.Helpers
{
    public static class WebhookHandler
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<bool> DeliverWebhookAsync(WebhookEvent evt, string endpoint, int maxRetries, ILogger logger)
        {
            string json = JsonConvert.SerializeObject(evt);
            string checksum = HashUtils.ComputeSha256(json);
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Add("X-Signature", checksum);
                    request.Headers.Add("X-Event-Type", evt.EventType);

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation($"Webhook delivered to {endpoint} successfully.");
                        return true;
                    }

                    logger.LogWarning($"Webhook delivery failed with status: {response.StatusCode}. Retrying...");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Webhook delivery error for event {evt.EventType} to {endpoint}");
                }

                retryCount++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // exponential backoff
            }

            logger.LogError($"Webhook delivery to {endpoint} failed after {maxRetries} retries.");
            return false;
        }
    }

    public class WebhookEvent
    {
        public IEnumerable<string> EventType { get; set; }
    }
}
