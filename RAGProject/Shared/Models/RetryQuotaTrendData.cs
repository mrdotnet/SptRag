namespace Spt.Rag.Shared.Models;

public class RetryQuotaTrendData
{
    public string User { get; set; }
    public string EndpointCategory { get; set; }
    public List<string> Timestamps { get; set; } = new();
    public List<int> RetryCounts { get; set; } = new();
}