using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

[ApiController]
[Route("[controller]")]
public class IngestController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public IngestController(IHttpClientFactory factory, IConfiguration config)
    {
        _httpClient = factory.CreateClient();
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Post(IFormFile file)
    {
        var url = _config["Functions:IngestUrl"]; // e.g., https://lightrag-api.azurewebsites.net/api/ingestion

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        var response = await _httpClient.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        return response.IsSuccessStatusCode ? Ok(result) : StatusCode((int)response.StatusCode, result);
    }
}
