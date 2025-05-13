using Newtonsoft.Json;
using StackExchange.Redis;

namespace Spt.Rag.Shared.Storage;

public static class EmbeddingFallbackStore
{
    public static async Task SaveToRedisFallback(IDatabase redis, string hash, float[] embedding)
    {
        await redis.StringSetAsync($"embedding:fallback:{hash}", JsonConvert.SerializeObject(embedding),
            TimeSpan.FromDays(2));
    }
}