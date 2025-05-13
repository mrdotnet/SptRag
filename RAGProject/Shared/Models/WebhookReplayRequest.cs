namespace Spt.Rag.Shared.Models;

public class WebhookReplayRequest
{
    public WebhookPayload Payload { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
}