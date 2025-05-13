namespace Spt.Rag.Shared.Models;

public class RetryPayload
{
    public string Payload { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public string LastError { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}