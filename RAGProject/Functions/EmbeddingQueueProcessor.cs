using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI;
using Spt.Rag.Shared.Models;
using Spt.Rag.Shared.Utils;

namespace SptRAG.Functions
{
    public class EmbeddingQueueProcessor
    {
        private readonly OpenAIClient _openAI;
        private readonly ILogger _logger;

        public EmbeddingQueueProcessor(OpenAIClient openAI, ILoggerFactory loggerFactory)
        {
            _openAI = openAI;
            _logger = loggerFactory.CreateLogger<EmbeddingQueueProcessor>();
        }

        [Function("ProcessEmbeddingQueue")]
        public async Task RunAsync([QueueTrigger("embedding-queue", Connection = "AzureWebJobsStorage")] string queueItem)
        {
            var request = JsonConvert.DeserializeObject<EmbeddingRequest>(queueItem);
            _logger.LogInformation($"Processing embedding for: {request.FileName} ({request.Hash})");

            string content = await BlobHelper.FetchTextFromBlobAsync(request.BlobUri);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogError($"No content retrieved from blob for {request.FileName}.");
                await WebhookHelper.NotifyFailure("embedding", request.FileName, request.Hash, "Empty content");
                return;
            }

            var embedding = await _openAI.GetEmbeddingsAsync("text-embedding-ada-002", content);
            if (embedding == null)
            {
                _logger.LogError($"Embedding generation failed for {request.FileName}.");
                await WebhookHelper.NotifyFailure("embedding", request.FileName, request.Hash, "OpenAI error");
                return;
            }

            await SqlHelper.StoreEmbeddingAsync(request.FileName, request.Hash, embedding, request.BlobUri);
            await SignalRHelper.SendStageAsync(request.FileName, request.Hash, "Embedded");
        }
    }
}
