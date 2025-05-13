using Spt.Rag.Shared.Models;

namespace Spt.Rag.Shared.Services;

public class AdminService
{
    public async Task<List<DocumentSummary>> GetRecentUploadsAsync()
    {
        await Task.Delay(200); // Simulate async call
        return new List<DocumentSummary>
        {
            new() { FileName = "Q1-Financials.pdf", UploadedBy = "alice", UploadedAt = DateTime.UtcNow },
            new() { FileName = "2025-Strategy.docx", UploadedBy = "bob", UploadedAt = DateTime.UtcNow }
        };
    }

    public async Task<List<WebhookFailure>> GetRecentWebhookFailuresAsync()
    {
        await Task.Delay(200);
        return new List<WebhookFailure>
        {
            new()
            {
                EventType = "document.validation.failed", Status = "Timeout", PayloadHash = "abc123",
                Timestamp = DateTime.UtcNow
            }
        };
    }

    public async Task UpdateReplayPayloadAsync(string hash, string json)
    {
        // Simulate logic
        await Task.Delay(100);
    }

    public async Task<List<ReplayQueueItem>> GetReplayQueueAsync()
    {
        await Task.Delay(200);
        return new List<ReplayQueueItem>
        {
            new() { EndpointName = "audit.sink", RetryCount = 2, EnqueuedAt = DateTime.UtcNow.AddMinutes(-10) }
        };
    }

    public async Task<List<RetryQuotaEntry>> GetAllRetryQuotasAsync(string selectedRole = null)
    {
        //TODO: Filter based on role
        return new List<RetryQuotaEntry>
        {
            new() { User = "alice", Role = "Admin", EndpointCategory = "signature-monitoring", Used = 3, Max = 10 },
            new() { User = "bob", Role = "User", EndpointCategory = "document-events", Used = 8, Max = 8 }
        };
    }

    public Task ResetRetryQuotaAsync(string user, string category)
    {
        return Task.CompletedTask;
    }

    public Task OverrideRetryQuotaAsync(string user, string category, int newMax)
    {
        return Task.CompletedTask;
    }

    public async Task<List<ReplayEvent>> GetReplayEventsAsync(string status = "")
    {
        await Task.Delay(100);
        var all = new List<ReplayEvent>
        {
            new()
            {
                EventType = "doc.uploaded", PayloadHash = "abc123", Status = "Failed", RetryCount = 1,
                Endpoint = "audit.sink"
            },
            new()
            {
                EventType = "signature.verified", PayloadHash = "def456", Status = "Pending", RetryCount = 0,
                Endpoint = "sig.monitor"
            }
        };
        return string.IsNullOrEmpty(status) ? all : all.Where(e => e.Status == status).ToList();
    }

    public async Task ReplayEventAsync(ReplayEvent evt)
    {
        await Task.Delay(50);
    }

    public Task AssignRoleAsync(string user, string role)
    {
        throw new NotImplementedException();
    }

    public Task RevokeRoleAsync(string user, string role)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetAuditLogsAsync(string type, string user)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetAllRolesAsync()
    {
        return Task.FromResult(new List<string> { "Admin", "User", "Auditor" });
    }

    public Task<List<string>> GetSignatureLogsAsync(string documentId)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetPublicKeyChainAsync(string documentId)
    {
        throw new NotImplementedException();
    }

    public Task RetryFailedWebhookAsync(string payloadHash)
    {
        throw new NotImplementedException();
    }

    public Task ExportQuotaToCsvAsync(RetryQuotaEntry entry)
    {
        throw new NotImplementedException();
    }

    public Task UpdateQuotaTTLAsync(string user, string category, int ttlHours)
    {
        return Task.CompletedTask;
    }

    public Task<RetryQuotaTrendData> GetRetryQuotaTrendAsync(string user, string category)
    {
        return Task.FromResult(new RetryQuotaTrendData
        {
            User = user,
            EndpointCategory = category,
            Timestamps =
                Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd")).ToList(),
            RetryCounts = [1, 3, 2, 4, 5, 3, 1]
        });
    }

    public Task<List<string>> GetUsersAsync()
    {
        return Task.FromResult(new List<string> { "alice", "bob", "charlie" });
    }

    public Task<List<string>> GetEndpointCategoriesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "document-events",
            "signature-monitoring",
            "audit.sink"
        });
    }

}