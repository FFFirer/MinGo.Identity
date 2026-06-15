using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Trace;
using MinGo.IdServer.Admin;
using MinGo.IdServer.Admin.Components;

var builder = WebApplication.CreateBuilder(args);

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
    if(idserverAdminConfiguration.OpenTelemetryOptions == null)
    {
        return;
    }

    if(idserverAdminConfiguration.OpenTelemetryOptions.EnableConsoleExporter)
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
if(!string.IsNullOrWhiteSpace(idserverAdminConfiguration.DataProtectionPath))
{
    adminBuilder.PersistDataprotection(idserverAdminConfiguration.DataProtectionPath);
}

if(idserverAdminConfiguration.IsRealmEnabled)
{
    adminBuilder.EnableRealm();
}

if (idserverAdminConfiguration.ForceHttps)
{
    adminBuilder.ForceHttps();
}

adminBuilder.UpdateOpenid(idserverAdminConfiguration.ClientId, idserverAdminConfiguration.ClientSecret, idserverAdminConfiguration.Scopes, idserverAdminConfiguration.IgnoreCertificateError);
// 反向代理转发头配置：在 YARP 环境下还原客户端原始域名和协议
// 需与 SimpleIdServer 的 IsForwardedEnabled 开关保持一致
if (idserverAdminConfiguration.IsForwardedEnabled)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                 | ForwardedHeaders.XForwardedProto
                                 | ForwardedHeaders.XForwardedHost;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

if (idserverAdminConfiguration.IsForwardedEnabled)
{
    app.UseForwardedHeaders();
}

app.UseIdserverAdmin();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();