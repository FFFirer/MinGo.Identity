using MinGo.IdServer.Admin.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Blazor Interactive Server setup
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Admin configuration
var adminConfig = builder.Configuration.Get<IdentityServerAdminConfiguration>()
    ?? throw new InvalidOperationException("Admin configuration not found.");

// Configure SimpleIdServer Admin
var adminBuilder = builder.AddIdserverAdmin(adminConfig.IdserverBaseUrl, o =>
{
    o.ScimUrl = adminConfig.ScimBaseUrl;
});

if (adminConfig.IsRealmEnabled)
{
    adminBuilder.EnableRealm();
}

if (adminConfig.ForceHttps)
{
    adminBuilder.ForceHttps();
}

adminBuilder.UpdateOpenid(
    adminConfig.ClientId,
    adminConfig.ClientSecret,
    adminConfig.Scopes,
    adminConfig.IgnoreCertificateError);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseIdserverAdmin();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapRazorComponents<SimpleIdServer.IdServer.Website.App>()
    .AddInteractiveServerRenderMode();

app.Run();
