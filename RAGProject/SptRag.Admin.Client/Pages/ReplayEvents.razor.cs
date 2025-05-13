using Microsoft.AspNetCore.Components;
using Radzen;
using Spt.Rag.Shared.Models;
using Spt.Rag.Shared.Services;
using SptRag.Admin.Client.Components;

public class ReplayEventsBase : ComponentBase
{
    [Inject] protected AdminService AdminService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

    protected List<ReplayEvent> Events = new();
    protected string SelectedStatus = "";

    protected IEnumerable<string> Statuses => Events.Select(e => e.Status).Distinct();

    protected IEnumerable<ReplayEvent> FilteredEvents =>
        string.IsNullOrWhiteSpace(SelectedStatus)
            ? Events
            : Events.Where(e => e.Status == SelectedStatus);

    protected override async Task OnInitializedAsync() => await LoadReplayEvents();

    protected async Task LoadReplayEvents()
    {
        Events = await AdminService.GetReplayEventsAsync(SelectedStatus);
        StateHasChanged();
    }

    protected async Task TriggerReplay(ReplayEvent evt)
    {
        await AdminService.ReplayEventAsync(evt);
        await LoadReplayEvents();
    }

    protected async Task EditPayload(ReplayEvent evt)
    {
        var parameters = new Dictionary<string, object>
        {
            { "PayloadHash", evt.PayloadHash },
            { "Json", evt.PayloadJson }
        };

        var result = await DialogService.OpenAsync<EditPayloadDialog>(
            "Edit Payload", parameters,
            new DialogOptions { Width = "600px", Height = "400px" });

        if (result is string newJson)
        {
            await AdminService.UpdateReplayPayloadAsync(evt.PayloadHash, newJson);
            await LoadReplayEvents();
        }
    }
}