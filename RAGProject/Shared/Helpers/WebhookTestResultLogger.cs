using Newtonsoft.Json;
using StackExchange.Redis;

namespace Spt.Rag.Shared.Helpers;

public static class WebhookTestResultLogger
{
    public static async Task LogResultAsync(IDatabase redis, string category, string hash, object payload)
    {
        string key = $"webhook:test:{category}:{hash}";
        string json = JsonConvert.SerializeObject(payload);
        await redis.StringSetAsync(key, json, TimeSpan.FromDays(14));
    }
}