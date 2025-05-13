using Spt.Rag.Shared.Models;

namespace SptRag.Admin.Client.Pages;

public partial class RetryTrends
{
    private List<TrendPoint> ChartData = new();
    private List<DropdownOption> EndpointCategories = new();
    private string SelectedCategory;
    private string SelectedUser;
    private RetryQuotaTrendData TrendData;
    private List<DropdownOption> Users = new();

    protected override async Task OnInitializedAsync()
    {
        var quotas = await AdminService.GetAllRetryQuotasAsync();
        Users = quotas.Select(q => new DropdownOption { Text = q.User, Value = q.User }).Distinct().ToList();
        EndpointCategories = quotas
            .Select(q => new DropdownOption { Text = q.EndpointCategory, Value = q.EndpointCategory }).Distinct()
            .ToList();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadTrendData();
    }

    private async Task LoadTrendData()
    {
        if (!string.IsNullOrEmpty(SelectedUser) && !string.IsNullOrEmpty(SelectedCategory))
        {
            TrendData = await AdminService.GetRetryQuotaTrendAsync(SelectedUser, SelectedCategory);

            ChartData = TrendData.Timestamps
                .Select((date, index) => new TrendPoint
                {
                    Date = date,
                    Count = TrendData.RetryCounts.ElementAtOrDefault(index)
                })
                .ToList();
        }
    }

    public class TrendPoint
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class DropdownOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}