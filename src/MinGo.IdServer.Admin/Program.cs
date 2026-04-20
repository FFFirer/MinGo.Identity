using SimpleIdServer.IdServer.Admin;

var builder = WebApplication.CreateBuilder(args);

// Get IdServer base URL
var idServerBaseUrl = builder.Configuration["IdServer:BaseUrl"] ?? "https://localhost:5001";

// Configure Admin services
builder.Services.AddSidAdmin(options =>
{
    options.IdServerBaseUrl = idServerBaseUrl;
    options.IsReamEnabled = true;
});

// Configure OIDC authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = "MinGo.Admin.Session";
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = idServerBaseUrl;
    options.ClientId = "admin_client";
    options.ClientSecret = "admin_secret";
    options.ResponseType = "code";
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run("https://localhost:5002");
