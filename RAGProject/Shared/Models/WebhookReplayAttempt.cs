namespace Spt.Rag.Shared.Models
{
    public class WebhookReplayAttempt
    {
        public string WebhookId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}