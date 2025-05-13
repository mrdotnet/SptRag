using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Spt.Rag.Shared.Utils;
using StackExchange.Redis;

namespace SptRAG.Functions
{
    public class ProcessEmbeddingQueueFunction
    {
        private readonly SqlConnection _sql;
        private readonly IDatabase _redis;
        private readonly ILogger _logger;

        public ProcessEmbeddingQueueFunction(SqlConnection sql, IConnectionMultiplexer redis, ILoggerFactory loggerFactory)
        {
            _sql = sql;
            _redis = redis.GetDatabase();
            _logger = loggerFactory.CreateLogger<ProcessEmbeddingQueueFunction>();
        }

        [Function("ProcessEmbeddingQueue")]
        public async Task RunAsync([QueueTrigger("embedding-queue", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            var embeddingRequest = EmbeddingHelper.ParseQueueMessage(queueMessage);

            if (!RbacHelper.HasPermission(embeddingRequest.UploadedBy, "Embed"))
            {
                _logger.LogWarning($"RBAC denied embedding for {embeddingRequest.FileName}");
                await AuditLogger.LogEmbeddingRejection(_sql, embeddingRequest);
                return;
            }

            _logger.LogInformation($"Generating embedding for {embeddingRequest.FileName}");
            var embedding = await EmbeddingHelper.GenerateEmbeddingAsync(embeddingRequest.BlobUri);

            var cmd = _sql.CreateCommand();
            cmd.CommandText = @"
                UPDATE Documents
                SET Embedding = @Embedding, EmbeddedAt = @EmbeddedAt
                WHERE Hash = @Hash";

            cmd.Parameters.AddWithValue("@Embedding", embedding);
            cmd.Parameters.AddWithValue("@EmbeddedAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Hash", embeddingRequest.Hash);

            await cmd.ExecuteNonQueryAsync();

            await _redis.StringSetAsync($"embedding:status:{embeddingRequest.Hash}", "completed", TimeSpan.FromHours(1));
            _logger.LogInformation($"Embedding stored and status updated for {embeddingRequest.FileName}");
        }
    }

    public class EmbeddingHelper
    {
        public static async Task<object> GenerateEmbeddingAsync(object blobUri)
        {
            throw new NotImplementedException();
        }

        public static object ParseQueueMessage(string queueMessage)
        {
            throw new NotImplementedException();
        }
    }
}
