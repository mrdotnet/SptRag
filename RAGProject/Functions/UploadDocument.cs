// File: DocumentUploadFunction.cs

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SptRAG.Functions;

public class DocumentUploadFunction
{
    private readonly ILogger _logger;
    private readonly IDatabase _redis;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly IQueueClient _queueClient;

    public DocumentUploadFunction(ILoggerFactory loggerFactory, IConnectionMultiplexer redis, BlobServiceClient blobServiceClient, IQueueClient queueClient)
    {
        _logger = loggerFactory.CreateLogger<DocumentUploadFunction>();
        _redis = redis.GetDatabase();
        _blobContainerClient = blobServiceClient.GetBlobContainerClient("documents");
        _queueClient = queueClient;
    }

    [Function("UploadDocument")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")] HttpRequestData req)
    {
        var form = await req.ReadFormAsync();
        var file = form.Files["file"];
        var userId = form["userId"];
        var timestamp = DateTime.UtcNow;

        if (file == null || file.Length == 0)
        {
            return CreateResponse(req, System.Net.HttpStatusCode.BadRequest, "Missing or empty file");
        }

        var fileHash = await FileHasher.ComputeBlake3Async(file.OpenReadStream());
        var redisKey = $"dedup:{fileHash}";

        if (await _redis.KeyExistsAsync(redisKey))
        {
            await AlertService.SendDuplicateAlertAsync(file.FileName, fileHash, userId);
            return CreateResponse(req, System.Net.HttpStatusCode.Conflict, "Duplicate file detected");
        }

        var blobName = $"{userId}/{Guid.NewGuid()}_{file.FileName}";
        await _blobContainerClient.UploadBlobAsync(blobName, file.OpenReadStream());

        var metadata = new
        {
            hash = fileHash,
            fileName = file.FileName,
            userId,
            blobUri = _blobContainerClient.GetBlobClient(blobName).Uri.ToString(),
            timestamp,
            mimeType = file.ContentType
        };

        await _queueClient.SendMessageAsync(JsonSerializer.Serialize(metadata));
        await _redis.StringSetAsync(redisKey, JsonSerializer.Serialize(metadata), TimeSpan.FromHours(1));

        return CreateResponse(req, System.Net.HttpStatusCode.Accepted, "File queued successfully");
    }

    private HttpResponseData CreateResponse(HttpRequestData req, System.Net.HttpStatusCode status, string message)
    {
        var response = req.CreateResponse(status);
        response.WriteString(message);
        return response;
    }
}