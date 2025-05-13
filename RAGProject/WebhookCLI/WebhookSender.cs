using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class WebhookSender
{
    public static async Task<bool> SendAsync(string url, string jsonPayload, string signature)
    {
        using var http = new HttpClient();

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        content.Headers.Add("X-Signature", signature);

        var response = await http.PostAsync(url, content);
        return response.IsSuccessStatusCode;
    }
}