using OpenTelemetry.Trace;
using Serilog;
using MinGo.IdServer.Admin;
using MinGo.IdServer.Admin.Components;
using MinGo.Identity.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Serilog: 接管日志管线，配置由 appsettings 驱动
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApiDocument();

builder.Configuration.AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
.AddEnvironmentVariables();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var idserverAdminConfiguration = builder.Configuration.Get<IdentityServerAdminConfiguration>() ?? throw new InvalidOperationException("Configuration section 'IdentityServerAdminConfiguration' is missing");
var adminBuilder = builder.AddIdserverAdmin(idserverAdminConfiguration.IdserverBaseUrl, o =>
{
    o.ScimUrl = idserverAdminConfiguration.ScimBaseUrl;
}).EnableOpenTelemetry(c =>
{
    if (idserverAdminConfiguration.OpenTelemetryOptions == null)
    {
        return;
    }

    if (idserverAdminConfiguration.OpenTelemetryOptions.EnableConsoleExporter)
    {
        c.AddConsoleExporter();
    }

    if (idserverAdminConfiguration.OpenTelemetryOptions.EnableOtpExported)
    {
        c.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(idserverAdminConfiguration.OpenTelemetryOptions.TracesEndpoint);
            o.Headers = idserverAdminConfiguration.OpenTelemetryOptions.Headers;
            o.Protocol = idserverAdminConfiguration.OpenTelemetryOptions.Protocol;
        });
    }
});
if (!string.IsNullOrWhiteSpace(idserverAdminConfiguration.DataProtectionPath))
{
    adminBuilder.PersistDataprotection(idserverAdminConfiguration.DataProtectionPath);
}

if (idserverAdminConfiguration.IsRealmEnabled)
{
    adminBuilder.EnableRealm();
}

if (idserverAdminConfiguration.ForceHttps)
{
    adminBuilder.ForceHttps();
}

adminBuilder.UpdateOpenid(idserverAdminConfiguration.ClientId, idserverAdminConfiguration.ClientSecret, idserverAdminConfiguration.Scopes, idserverAdminConfiguration.IgnoreCertificateError);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Serve static files from wwwroot/ (_framework/blazor.web.js, _content/*, lib/*)
// Required in Production/Docker; Development mode relies on SDK Static Web Assets middleware.
app.UseStaticFiles();

app.UseIdserverAdmin();

if (idserverAdminConfiguration.ForceHttps)
{
    app.UseHttpsRedirection();
}

// app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();