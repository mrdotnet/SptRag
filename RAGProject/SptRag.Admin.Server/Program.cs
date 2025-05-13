using Radzen;
using SptRag.Admin.Server.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.OData;
using SptRag.Admin.Server.Data;
using Microsoft.AspNetCore.Identity;
using SptRag.Admin.Server.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Spt.Rag.Shared.Services;
using SptRag.Admin.Client;
using SptRag.Admin.Client.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024).AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "SptRag.Admin.Theme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<SptRag.Admin.Server.Services.SptRagDbService>();
builder.Services.AddDbContext<SptRag.Admin.Server.Data.SptRagDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SptRagDbConnection"));
});
builder.Services.AddControllers().AddOData(opt =>
{
    var oDataBuilderSptRagDb = new ODataConventionModelBuilder();
    oDataBuilderSptRagDb.EntitySet<Document>("Documents");
    opt.AddRouteComponents("odata/SptRagDb", oDataBuilderSptRagDb.GetEdmModel()).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null).TimeZone = TimeZoneInfo.Utc;
});
builder.Services.AddScoped<SptRagDbService>();
builder.Services.AddScoped<AdminService>(); // needed here because we use auto render mode
builder.Services.AddHttpClient("SptRag.Admin.Server").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false }).AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SptRagDbConnection"));
});
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationIdentityDbContext>().AddDefaultTokenProviders();
builder.Services.AddTransient<IUserStore<ApplicationUser>, MultiTenancyUserStore>();
builder.Services.AddControllers().AddOData(o =>
{
    var oDataBuilder = new ODataConventionModelBuilder();
    oDataBuilder.EntitySet<ApplicationUser>("ApplicationUsers");
    var usersType = oDataBuilder.StructuralTypes.First(x => x.ClrType == typeof(ApplicationUser));
    usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Password)));
    usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.ConfirmPassword)));
    oDataBuilder.EntitySet<ApplicationRole>("ApplicationRoles");
    oDataBuilder.EntitySet<ApplicationTenant>("ApplicationTenants");
    o.AddRouteComponents("odata/Identity", oDataBuilder.GetEdmModel()).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null).TimeZone = TimeZoneInfo.Utc;
});
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationAuthenticationStateProvider>();
var app = builder.Build();
var forwardingOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardingOptions.KnownNetworks.Clear();
forwardingOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardingOptions);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseHeaderPropagation();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(typeof(SptRag.Admin.Client._Imports).Assembly);
app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>().Database.Migrate();
app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>().SeedTenantsAdmin().Wait();
app.Run();