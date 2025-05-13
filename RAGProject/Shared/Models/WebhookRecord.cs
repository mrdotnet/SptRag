namespace Spt.Rag.Shared.Models
{
    public class WebhookRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string EventType { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int RetryCount { get; set; }
        public bool Delivered { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string Category { get; set; } = "default";
    }
}