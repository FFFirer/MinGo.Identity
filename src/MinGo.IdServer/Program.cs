using DataSeeder;
using Microsoft.EntityFrameworkCore;
using MinGo.IdServer.Seed;
using SimpleIdServer.IdServer;
using SimpleIdServer.IdServer.Builders;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("IdServer")
    ?? throw new InvalidOperationException("Connection string 'IdServer' not found.");

var authority = builder.Configuration["Authority"] ?? "https://localhost:5001";

// Configure SimpleIdServer with PostgreSQL (7.0.0 API)
var idServerBuilder = builder.AddSidIdentityServer(c =>
{
    c.Authority = authority;
})
    .AddDeveloperSigningCredential()
    .UseEfStore(
        idStore => idStore.UseNpgsql(connectionString, o =>
        {
            o.MigrationsAssembly("SimpleIdServer.IdServer.PostgreMigrations");
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }),
        fbStore => fbStore.UseNpgsql(connectionString, o =>
        {
            o.MigrationsAssembly("SidFormBuilder.PostgreMigrations");
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
    )
    .AddPwdAuthentication(true)
    .AddEmailAuthentication()
    .AddSwagger(opt =>
    {
        opt.IncludeDocumentation<SimpleIdServer.IdServer.TokenTypes.AccessTokenTypeService>();
    });

// Register custom seed data
builder.Services.AddTransient<IDataSeeder, AdminUserDataSeeder>();

var app = builder.Build();

// Apply seed data
app.Services.SeedData();

app.UseSid()
    .UseSidSwagger()
    .UseSidSwaggerUi();

app.Run();
