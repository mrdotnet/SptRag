namespace Spt.Rag.Shared.Models
{
    public class ReplayEvent
    {
        public string EventType { get; set; }
        public string PayloadHash { get; set; }
        public string PayloadJson { get; set; }
        public string Status { get; set; }
        public int RetryCount { get; set; }
        public string Endpoint { get; set; }
    }
}