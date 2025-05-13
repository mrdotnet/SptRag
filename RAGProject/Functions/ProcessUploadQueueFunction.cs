using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Spt.Rag.Shared.Models;
using Spt.Rag.Shared.Utils;

namespace SptRAG.Functions
{
    public class ProcessUploadQueueFunction
    {
        private readonly BlobServiceClient _blobClient;
        private readonly IDatabase _redis;
        private readonly SqlConnection _sql;
        private readonly ILogger _logger;
        private readonly HubConnection _signalR;

        public ProcessUploadQueueFunction(BlobServiceClient blobClient,
                                          IConnectionMultiplexer redis,
                                          SqlConnection sql,
                                          ILoggerFactory loggerFactory)
        {
            _blobClient = blobClient;
            _redis = redis.GetDatabase();
            _sql = sql;
            _logger = loggerFactory.CreateLogger<ProcessUploadQueueFunction>();
            _signalR = new HubConnectionBuilder()
                .WithUrl(Environment.GetEnvironmentVariable("SignalRHubUrl"))
                .Build();
            _signalR.StartAsync().Wait();
        }

        [Function("ProcessUploadQueue")]
        public async Task RunAsync([QueueTrigger("doc-upload-queue", Connection = "AzureWebJobsStorage")] string message)
        {
            var upload = JsonConvert.DeserializeObject<DocumentUploadMessage>(message);
            _logger.LogInformation($"Processing document {upload.FileName} from {upload.UploadedBy}");

            string fileHash = HashUtils.ComputeBlake3(upload.Base64Content);

            bool isDuplicate = await _redis.StringGetAsync($"doc:hash:{fileHash}") != RedisValue.Null;

            if (isDuplicate)
            {
                _logger.LogWarning($"Duplicate detected: {fileHash}");
                await AuditLogger.LogDuplicate(_sql, upload, fileHash);
                await _signalR.SendAsync("StageProgress", new
                {
                    FileName = upload.FileName,
                    Hash = fileHash,
                    Stage = "DuplicateDetected",
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            await _signalR.SendAsync("StageProgress", new
            {
                FileName = upload.FileName,
                Hash = fileHash,
                Stage = "Parsed",
                Timestamp = DateTime.UtcNow
            });

            var container = _blobClient.GetBlobContainerClient("documents");
            var blobName = $"{Guid.NewGuid()}_{upload.FileName}";
            var blobClient = container.GetBlobClient(blobName);

            await blobClient.UploadAsync(BinaryData.FromBytes(Convert.FromBase64String(upload.Base64Content)));
            var blobUri = blobClient.Uri.ToString();

            var command = _sql.CreateCommand();
            command.CommandText = @"
                INSERT INTO Documents (FileName, MimeType, Language, Hash, UploadedBy, BlobUri, UploadedAt)
                VALUES (@FileName, @MimeType, @Language, @Hash, @UploadedBy, @BlobUri, @UploadedAt)";

            command.Parameters.AddWithValue("@FileName", upload.FileName);
            command.Parameters.AddWithValue("@MimeType", upload.MimeType);
            command.Parameters.AddWithValue("@Language", upload.Language);
            command.Parameters.AddWithValue("@Hash", fileHash);
            command.Parameters.AddWithValue("@UploadedBy", upload.UploadedBy);
            command.Parameters.AddWithValue("@BlobUri", blobUri);
            command.Parameters.AddWithValue("@UploadedAt", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync();

            await _redis.StringSetAsync($"doc:hash:{fileHash}", blobUri, TimeSpan.FromDays(30));

            var embeddingQueued = await QueueHelper.TryEnqueueEmbeddingRequest(upload.FileName, fileHash, blobUri);

            if (embeddingQueued)
            {
                _logger.LogInformation($"Document {upload.FileName} queued for embedding.");

                await _signalR.SendAsync("StageProgress", new
                {
                    FileName = upload.FileName,
                    Hash = fileHash,
                    Stage = "QueuedForEmbedding",
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogError($"Embedding queue failed. Routing to retry.");
                await RedisQueueHelper.PushToRetryQueue("embedding-retry", new
                {
                    upload.FileName,
                    Hash = fileHash,
                    Uri = blobUri,
                    RetryCount = 0
                });

                await WebhookHelper.NotifyFailure("embedding", upload.FileName, fileHash, "Retry enqueued");
            }
        }
    }
}
