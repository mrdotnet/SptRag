using System.Net.Http.Json;
using Spt.Rag.Shared.Models;

namespace SptRag.Admin.Client.Services;

public class EditPayloadService
{
    private readonly HttpClient _http;

    public EditPayloadService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PayloadModel> GetPayloadAsync(string hash)
    {
        return await _http.GetFromJsonAsync<PayloadModel>($"/api/replay/payload/{hash}");
    }

    public async Task UpdatePayloadAsync(string hash, string json, string annotation)
    {
        var payload = new { json, annotation };
        await _http.PutAsJsonAsync($"/api/replay/payload/{hash}", payload);
    }
}