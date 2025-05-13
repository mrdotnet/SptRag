using Spt.Rag.Shared.Models;

namespace SptRag.Admin.Client.Pages;

public partial class ReplayQueue
{
    private List<ReplayQueueItem> queue = new();

    protected override async Task OnInitializedAsync()
    {
        queue = await AdminService.GetReplayQueueAsync();
    }

    private async Task Replay(ReplayQueueItem item)
    {
        await AdminService.ReplayEventAsync(new ReplayEvent
        {
            Endpoint = item.EndpointName,
            RetryCount = item.RetryCount,
            Status = "Queued",
            PayloadHash = Guid.NewGuid().ToString() // Simulated
        });

        await LoadData();
    }

    private async Task LoadData()
    {
        queue = await AdminService.GetReplayQueueAsync();
        StateHasChanged();
    }

    private void OnSelect(ReplayQueueItem item) => Console.WriteLine($"Selected: {item.EndpointName}");
}