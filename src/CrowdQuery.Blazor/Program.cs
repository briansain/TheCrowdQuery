using CrowdQuery.AS;
using CrowdQuery.Blazor.Components;
using MudBlazor.Services;
using Serilog;
using static CrowdQuery.AS.ServiceCollectionExtension;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext());

builder.Host.ConfigureServices((context, services) => {
    services
        .AddCrowdQueryAkka(context.Configuration, [ClusterConstants.MainNode, ClusterConstants.ProjectionNode])
        .AddMudServices()
        .AddRazorComponents()
        .AddInteractiveServerComponents();;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
