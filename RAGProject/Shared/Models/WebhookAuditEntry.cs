namespace Spt.Rag.Shared.Models
{
    public class WebhookAuditEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string WebhookId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // e.g. 'delivered', 'replayed', 'failed'
        public string Actor { get; set; } = "system"; // user id or 'system'
        public DateTime Timestamp { get; set; }
        public string Metadata { get; set; } = string.Empty; // JSON encoded details
    }
}