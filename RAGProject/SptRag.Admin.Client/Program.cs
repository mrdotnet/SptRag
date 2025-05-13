using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Spt.Rag.Shared.Services;
using SptRag.Admin.Client;
using SptRag.Admin.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");

builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "SptRag.Admin.Theme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddScoped<SptRagDbService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpClient("SptRag.Admin.Server",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SptRag.Admin.Server"));
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<AdminService>();
var host = builder.Build();
await host.RunAsync();