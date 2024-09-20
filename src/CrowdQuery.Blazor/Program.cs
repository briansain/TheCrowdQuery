using CrowdQuery.AS;
using CrowdQuery.Blazor.Components;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext());

builder.Configuration.AddJsonFile("appsettings.json", true);

// Add services to the container.
// builder.Services
//     .AddMudServices()
//     .AddRazorComponents()
//     .AddInteractiveServerComponents();

builder.Host.ConfigureServices((context, services) => {
    // Log.Logger = new LoggerConfiguration()
    //             .ReadFrom.Configuration(context.Configuration)
    //             .Enrich.FromLogContext()
    //             .CreateLogger();
    services
        // .AddLogging(config => config.AddSerilog())
        .AddCrowdQueryAkka(context.Configuration)
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
