using System.Net.Http.Json;

namespace SptRag.Admin.Client.Services;

public class ReplayQueueService
{
    private readonly HttpClient _http;

    public ReplayQueueService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ReplayQueueItem>> GetQueueAsync()
    {
        return await _http.GetFromJsonAsync<List<ReplayQueueItem>>("/api/replay/queue");
    }

    public async Task RetryAsync(string hash)
    {
        await _http.PostAsync($"/api/replay/queue/retry/{hash}", null);
    }

    public async Task RemoveAsync(string hash)
    {
        await _http.DeleteAsync($"/api/replay/queue/{hash}");
    }

    public class ReplayQueueItem
    {
        public string Hash { get; set; }
        public string Endpoint { get; set; }
        public int RetryCount { get; set; }
        public DateTime EnqueuedAt { get; set; }
    }
}