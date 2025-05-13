namespace Spt.Rag.Shared.Models
{
    public class DocumentUploadMessage
    {
        public string FileName { get; set; }
        public string OriginPath { get; set; }
        public string MimeType { get; set; }
        public string Language { get; set; }
        public string Base64Content { get; set; }
        public string UploadedBy { get; set; }
    }
}
