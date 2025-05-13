using System.Text;
using Newtonsoft.Json;

namespace Spt.Rag.Shared.Models;

public class AzureOpenAIEmbeddingModel : IEmbeddingModel
{
    private readonly HttpClient _client;

    public AzureOpenAIEmbeddingModel(HttpClient client)
    {
        _client = client;
    }

    public string ModelVersion => "text-embedding-ada-002";

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // Simplified Azure OpenAI call
        var request = new
        {
            input = text,
            model = ModelVersion
        };

        var response = await _client.PostAsync(
            Environment.GetEnvironmentVariable("AzureOpenAIEndpoint"),
            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
        return ((IEnumerable<object>)result.data[0].embedding).Select(Convert.ToSingle).ToArray();
    }
}