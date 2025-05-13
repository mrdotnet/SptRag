using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebhookUtility.Models;

namespace WebhookTestHarness
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Webhook Test Harness ===\n");

            var payload = new WebhookPayload
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = "document.uploaded",
                Timestamp = DateTime.UtcNow,
                DocumentId = "doc-123456",
                Metadata = new WebhookMetadata
                {
                    FileName = "sample.pdf",
                    MimeType = "application/pdf",
                    User = "test.user@example.com"
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync("https://localhost:7071/api/webhook/receive", content);

            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}