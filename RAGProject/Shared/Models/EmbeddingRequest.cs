namespace Spt.Rag.Shared.Models
{
    public class EmbeddingRequest
    {
        public string FileName { get; set; }
        public string Hash { get; set; }
        public string BlobUri { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
