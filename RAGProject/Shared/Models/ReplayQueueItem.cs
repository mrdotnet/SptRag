namespace Spt.Rag.Shared.Models
{
    public class ReplayQueueItem
    {
        public string EndpointName { get; set; }
        public int RetryCount { get; set; }
        public DateTime EnqueuedAt { get; set; }
    }
}