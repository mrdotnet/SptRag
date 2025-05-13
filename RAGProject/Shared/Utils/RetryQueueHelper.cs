using Newtonsoft.Json;
using Spt.Rag.Shared.Models;

namespace Spt.Rag.Shared.Utils
{
    public static class RetryQueueHelper
    {
        private static readonly string RetryPrefix = "retry:embedding:";
        private static readonly int MaxRetries = 5;
        private static readonly TimeSpan BaseDelay = TimeSpan.FromMinutes(1);

        public static async Task PushToRetryQueue(string queueName, object payload)
        {
            var redis = RedisConnector.Connection.GetDatabase();
            var wrapped = new RetryPayload
            {
                Payload = JsonConvert.SerializeObject(payload),
                Timestamp = DateTime.UtcNow,
                RetryCount = 0,
                LastError = "Initial enqueue"
            };

            string key = RetryPrefix + Guid.NewGuid();
            await redis.StringSetAsync(key, JsonConvert.SerializeObject(wrapped));
            await redis.ListRightPushAsync(queueName, key);
        }

        public static async Task<bool> HandleRetryAsync(string key, string queueName, string failureType)
        {
            var redis = RedisConnector.Connection.GetDatabase();
            var raw = await redis.StringGetAsync(key);
            if (raw.IsNullOrEmpty) return false;

            var wrapped = JsonConvert.DeserializeObject<RetryPayload>(raw);
            wrapped.RetryCount++;
            wrapped.LastError = failureType;
            wrapped.Timestamp = DateTime.UtcNow;

            if (wrapped.RetryCount > MaxRetries)
            {
                await redis.StringSetAsync("deadletter:" + key, JsonConvert.SerializeObject(wrapped));
                await redis.KeyDeleteAsync(key);
                await redis.HashIncrementAsync("metrics:retry:deadletter", failureType);
                return false;
            }

            var delay = TimeSpan.FromSeconds(Math.Pow(2, wrapped.RetryCount) * BaseDelay.TotalSeconds);
            await redis.StringSetAsync(key, JsonConvert.SerializeObject(wrapped), delay);
            await redis.ListRightPushAsync(queueName, key);

            await redis.HashIncrementAsync("metrics:retry:attempts", failureType);
            return true;
        }

        public static async Task PurgeDeadLettersAsync()
        {
            var redis = RedisConnector.Connection.GetDatabase();
            var server = RedisConnector.GetServer();
            foreach (var key in server.Keys(pattern: "deadletter:*"))
            {
                await redis.KeyDeleteAsync(key);
            }
        }

        public static async Task<RetryPayload?> GetRetryPayload(string key)
        {
            var redis = RedisConnector.Connection.GetDatabase();
            var raw = await redis.StringGetAsync(key);
            return raw.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<RetryPayload>(raw);
        }
    }
}
