using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using SptRag.Admin.Client.Services;
using SptRag.Admin.Server.Models;

namespace SptRag.Admin.Client.Pages;

public partial class AddApplicationRole
{
    protected string error;
    protected bool errorVisible;

    protected ApplicationRole role;

    [Inject] protected IJSRuntime JSRuntime { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; }

    [Inject] protected DialogService DialogService { get; set; }

    [Inject] protected TooltipService TooltipService { get; set; }

    [Inject] protected ContextMenuService ContextMenuService { get; set; }

    [Inject] protected NotificationService NotificationService { get; set; }

    [Inject] protected SecurityService Security { get; set; }

    protected override async Task OnInitializedAsync()
    {
        role = new ApplicationRole();
    }

    protected async Task FormSubmit(ApplicationRole role)
    {
        try
        {
            await Security.CreateRole(role);

            DialogService.Close();
        }
        catch (Exception ex)
        {
            errorVisible = true;
            error = ex.Message;
        }
    }

    protected async Task CancelClick()
    {
        DialogService.Close();
    }
}