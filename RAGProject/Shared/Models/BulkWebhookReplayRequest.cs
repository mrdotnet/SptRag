namespace Spt.Rag.Shared.Models;

public class BulkWebhookReplayRequest
{
    public List<WebhookReplayRequest> Requests { get; set; } = new();
}