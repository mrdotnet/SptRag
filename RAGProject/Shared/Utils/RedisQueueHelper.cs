using Newtonsoft.Json;
using StackExchange.Redis;

namespace Spt.Rag.Shared.Utils
{
    public static class RedisQueueHelper
    {
        private static readonly IDatabase _redis = RedisConnector.Connection.GetDatabase();

        public static async Task PushToRetryQueue(string queueName, object payload, int maxRetries = 5, int initialDelaySeconds = 30)
        {
            var entry = new RetryQueueEntry
            {
                Payload = JsonConvert.SerializeObject(payload),
                Attempt = 0,
                Timestamp = DateTime.UtcNow,
                MaxRetries = maxRetries,
                DelaySeconds = initialDelaySeconds
            };
            await _redis.ListLeftPushAsync(queueName, JsonConvert.SerializeObject(entry));
        }

        public static async Task<RetryQueueEntry?> GetNextRetry(string queueName)
        {
            var value = await _redis.ListRightPopAsync(queueName);
            if (value.IsNullOrEmpty) return null;

            var entry = JsonConvert.DeserializeObject<RetryQueueEntry>(value);
            if (entry == null) return null;

            var delay = TimeSpan.FromSeconds(Math.Pow(2, entry.Attempt) * entry.DelaySeconds);
            if (DateTime.UtcNow - entry.Timestamp < delay)
            {
                // Re-queue if not ready
                await _redis.ListRightPushAsync(queueName, value);
                return null;
            }

            entry.Attempt++;
            if (entry.Attempt > entry.MaxRetries)
            {
                await WebhookHelper.NotifyFailure("retry-cap", payload: entry.Payload, reason: "Max retries reached");
                await _redis.ListRightPushAsync($"{queueName}-dead", value);
                return null;
            }

            return entry;
        }
    }

    public class RetryQueueEntry
    {
        public string Payload { get; set; } = string.Empty;
        public int Attempt { get; set; }
        public DateTime Timestamp { get; set; }
        public int MaxRetries { get; set; }
        public int DelaySeconds { get; set; }
    }
}