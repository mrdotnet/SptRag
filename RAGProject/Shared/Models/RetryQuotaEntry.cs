namespace Spt.Rag.Shared.Models;

public class RetryQuotaEntry
{
    public string User { get; set; }
    public string Role { get; set; }
    public string EndpointCategory { get; set; }
    public int Used { get; set; }
    public int RetryCount { get; set; }
    public int Max { get; set; }
    public DateTime LastRetry { get; set; }
    public DateTime LastReset { get; set; }
    public int ResetTTLHours { get; set; } // Example: 24
}