using SimpleIdServer.IdServer;
using SimpleIdServer.IdServer.Builders;
using SimpleIdServer.IdServer.Stores;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("IdServer")!;

// Configure SimpleIdServer with PostgreSQL
builder.AddSidIdentityServer()
    .AddDeveloperSigningCredential()
    .AddPostgreSQL(connectionString)
    .AddPwdAuthentication(true)
    .AddEmailNotification(true);

var app = builder.Build();

// Seed data with default administrator
app.Services.SeedData((serviceProvider, dbContext) =>
{
    // Add default admin user if not exists
    var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    var existingUser = userRepository.GetBySubject("administrator").Result;
    
    if (existingUser == null)
    {
        var user = UserBuilder.Create("administrator", "password", "Administrator")
            .SetFirstname("Administrator")
            .SetEmail("admin@mingo.local")
            .Build();
        
        userRepository.Add(user).Wait();
    }
});

app.UseSid();

app.Run("https://localhost:5001");
