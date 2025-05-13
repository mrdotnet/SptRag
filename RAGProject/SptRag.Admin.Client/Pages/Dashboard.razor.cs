using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Spt.Rag.Shared.Models;
using Spt.Rag.Shared.Services;

namespace SptRag.Admin.Client.Pages;

public partial class Dashboard
{
    [Inject]
    protected AdminService AdminService { get; set; }
    
    private List<DocumentSummary> uploads = new();
    private List<WebhookFailure> failures = new();
    private List<ReplayQueueItem> replayQueue = new();

    protected override async Task OnInitializedAsync()
    {
        uploads = await AdminService.GetRecentUploadsAsync();
        failures = await AdminService.GetRecentWebhookFailuresAsync();
        replayQueue = await AdminService.GetReplayQueueAsync();
    }
}