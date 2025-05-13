using System.Text;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using Spt.Rag.Shared.Models;

namespace Spt.Rag.Shared.Utils;

public static class QueueHelper
{
    public static async Task EnqueueEmbeddingRequest(string fileName, string hash, string blobUri)
    {
        var embeddingRequest = new EmbeddingRequest
        {
            FileName = fileName,
            Hash = hash,
            BlobUri = blobUri,
            RequestedAt = DateTime.UtcNow
        };

        var queueClient = new QueueClient(
            Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
            "embedding-request-queue");

        await queueClient.SendMessageAsync(Convert.ToBase64String(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(embeddingRequest))));
    }
}