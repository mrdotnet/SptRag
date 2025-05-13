namespace Spt.Rag.Shared.Models;

public class WebhookReplayResult
{
    public string PayloadId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string>? Errors { get; set; }
}