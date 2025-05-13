namespace Spt.Rag.Shared.Models;

public class DocumentSummary
{
    public string FileName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}