namespace Spt.Rag.Shared.Models
{
    public class WebhookFailure
    {
        public string EventType { get; set; }
        public string Status { get; set; }
        public string PayloadHash { get; set; }
        public DateTime Timestamp { get; set; }
    }
}