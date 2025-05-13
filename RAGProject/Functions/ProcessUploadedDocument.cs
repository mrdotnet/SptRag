// File: Functions/ProcessUploadedDocument.cs

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SptRAG.Functions
;

public class ProcessUploadedDocument
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IDatabase _redis;
    private readonly IConfiguration _config;
    private readonly HubConnection _signalRConnection;

    public ProcessUploadedDocument(
        ILoggerFactory loggerFactory,
        BlobServiceClient blobServiceClient,
        IConnectionMultiplexer redisConnection,
        IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<ProcessUploadedDocument>();
        _blobServiceClient = blobServiceClient;
        _redis = redisConnection.GetDatabase();
        _config = config;

        _signalRConnection = new HubConnectionBuilder()
            .WithUrl(config["SignalR:HubUrl"])
            .WithAutomaticReconnect()
            .Build();

        _signalRConnection.StartAsync().GetAwaiter().GetResult();
    }

    [Function("ProcessUploadedDocument")]
    public async Task Run([QueueTrigger("document-uploads", Connection = "AzureWebJobsStorage")] string queueItem)
    {
        var metadata = JsonSerializer.Deserialize<DocumentMetadata>(queueItem);
        if (metadata == null || string.IsNullOrWhiteSpace(metadata.FileName))
        {
            _logger.LogWarning("Invalid message: {queueItem}", queueItem);
            return;
        }

        var container = _blobServiceClient.GetBlobContainerClient("documents");
        var blobClient = container.GetBlobClient(metadata.FileName);
        var blobStream = await blobClient.OpenReadAsync();

        await SendSignalRUpdate(metadata.DocumentId, "Parsed");

        // TODO: Parse the file according to MIME type and extract content
        var extractedText = await ParseDocumentAsync(blobStream, metadata.MimeType);

        await SendSignalRUpdate(metadata.DocumentId, "Embedded");

        // TODO: Call Azure OpenAI Embedding API (e.g., ada-002)
        var vector = await EmbedTextAsync(extractedText);

        await StoreInSqlAsync(metadata.DocumentId, vector, extractedText, metadata);

        await SendSignalRUpdate(metadata.DocumentId, "Indexed");

        // Optionally update Redis TTL
        await _redis.KeyExpireAsync("dedup:" + metadata.Hash, TimeSpan.FromHours(1));
    }

    private Task<string> ParseDocumentAsync(Stream blobStream, string mimeType)
    {
        // Placeholder logic for now
        using var reader = new StreamReader(blobStream);
        return Task.FromResult(reader.ReadToEnd());
    }

    private async Task<float[]> EmbedTextAsync(string text)
    {
        // Placeholder for actual call to Azure OpenAI
        return await Task.FromResult(new float[1536]); // Simulate 1536-dim embedding
    }

    private Task StoreInSqlAsync(string documentId, float[] vector, string text, DocumentMetadata metadata)
    {
        // Placeholder: Implement actual DB insert with vector support
        return Task.CompletedTask;
    }

    private async Task SendSignalRUpdate(string documentId, string stage)
    {
        await _signalRConnection.InvokeAsync("SendStatus", new
        {
            DocumentId = documentId,
            Stage = stage,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    private record DocumentMetadata
    {
        public string DocumentId { get; init; }
        public string FileName { get; init; }
        public string MimeType { get; init; }
        public string Hash { get; init; }
        public string UploadedBy { get; init; }
    }
}