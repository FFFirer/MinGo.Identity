using OpenTelemetry.Trace;
using Serilog;
using MinGo.IdServer.Admin;
using MinGo.IdServer.Admin.Components;
using Microsoft.AspNetCore.HttpOverrides;
using MinGo.Identity.Configuration;
using SimpleIdServer.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;

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

var publicBaseUrl = ValidateBaseUrl(idserverAdminConfiguration.IdserverBaseUrl, nameof(idserverAdminConfiguration.IdserverBaseUrl));
var backchannelBaseUrl = string.IsNullOrWhiteSpace(idserverAdminConfiguration.BackchannelBaseUrl)
    ? null : ValidateBaseUrl(idserverAdminConfiguration.BackchannelBaseUrl, nameof(idserverAdminConfiguration.BackchannelBaseUrl));
if (idserverAdminConfiguration.BackchannelTimeoutSeconds is < 1 or > 60)
    throw new InvalidOperationException("BackchannelTimeoutSeconds must be between 1 and 60.");

// Configure the custom options used by SimpleIdServer, before its OIDC post-configuration creates the client.
builder.Services.AddOptions<CustomOpenIdConnectOptions>("oidc").Configure<ILoggerFactory>((options, loggerFactory) =>
{
    options.BackchannelTimeout = TimeSpan.FromSeconds(idserverAdminConfiguration.BackchannelTimeoutSeconds);
    if (options.BackchannelHttpHandler is HttpClientHandler transport) transport.AllowAutoRedirect = false;
    options.BackchannelHttpHandler = new OidcBackchannelHandler(publicBaseUrl, backchannelBaseUrl,
        loggerFactory.CreateLogger<OidcBackchannelHandler>())
    {
        InnerHandler = options.BackchannelHttpHandler ?? new HttpClientHandler { AllowAutoRedirect = false }
    };
    var remoteFailure = options.Events.OnRemoteFailure;
    options.Events.OnRemoteFailure = async context =>
    {
        loggerFactory.CreateLogger("Admin.Authentication").LogWarning(
            "OIDC remote login failed: Path {Path}, ErrorType {ErrorType}, TraceId {TraceId}",
            context.Request.Path, context.Failure?.GetType().Name, context.HttpContext.TraceIdentifier);
        if (remoteFailure is not null) await remoteFailure(context);
    };
});

// ────────────────────────────────────────────────────────────────
// Forwarded headers — trust X-Forwarded-Host/Proto/For from YARP.
// Must be configured before builder.Build() so middleware pipeline
// picks it up. Aligns with IdServer's existing fix in Program.cs.
// ────────────────────────────────────────────────────────────────
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
app.Logger.LogInformation("Admin OIDC transport: Authority {Authority}, Backchannel {Backchannel}, TimeoutSeconds {TimeoutSeconds}",
    publicBaseUrl.GetLeftPart(UriPartial.Path), backchannelBaseUrl?.GetLeftPart(UriPartial.Path) ?? "public",
    idserverAdminConfiguration.BackchannelTimeoutSeconds);

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

if (!app.Environment.IsDevelopment())
{
    // Handle in-place: /Error is not a route in the upstream realm middleware and otherwise produces a second exception.
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        var failure = context.Features.Get<IExceptionHandlerFeature>();
        app.Logger.LogError("Admin request failed: Path {Path}, ErrorType {ErrorType}, TraceId {TraceId}",
            context.Request.Path, failure?.Error.GetType().Name, context.TraceIdentifier);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync($"登录服务暂时不可用，请稍后重试。参考编号：{context.TraceIdentifier}");
    }));
}

// Serve static files from wwwroot/ (_framework/blazor.web.js, _content/*, lib/*)
// Required in Production/Docker; Development mode relies on SDK Static Web Assets middleware.
app.UseStaticFiles();

if (idserverAdminConfiguration.IsForwardedEnabled)
{
    app.UseForwardedHeaders();
}

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

static Uri ValidateBaseUrl(string value, string name)
{
    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
        || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
        throw new InvalidOperationException($"{name} must be an absolute HTTP(S) base URL without credentials, query or fragment.");
    return uri;
}
