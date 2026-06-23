using System.CommandLine;
using DataSeeder;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinGo.IdServer;
using MinGo.IdServer.Conf;
using MinGo.Identity.Configuration;
using MinGo.Identity.Migrations;
using MinGo.Identity.Migrator;
using SimpleIdServer.IdServer.Store.EF;

var migrateCommand = new Command("migrate", "Apply all pending migrations and seed data");
var scriptCommand = new Command("script", "Generate migration SQL script");
var outputOption = new Option<FileInfo?>(
    "--output", "Output file path for SQL script");
scriptCommand.AddOption(outputOption);

var rootCommand = new RootCommand("MinGo Identity - Database Migration Tool")
{
    migrateCommand,
    scriptCommand
};

migrateCommand.SetHandler(async () =>
{
    Console.WriteLine("=== MinGo Identity Migration Tool ===");

    // 1. Build DI container (identical to IdServer via SidServerSetup)
    var builder = WebApplication.CreateBuilder();
    builder.Configuration
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
    var config = builder.Configuration.Get<IdentityServerConfiguration>()
        ?? throw new InvalidOperationException("Configuration section 'IdentityServerConfiguration' is missing");
    SidServerSetup.ConfigureIdServer(builder, config);

    // 2. Migrator-specific: 自定义扩展表迁移 + 种子
    var storageConfig = ConfigurationHelper.LoadStorageConfiguration(builder.Configuration);
    Console.WriteLine($"Storage: {storageConfig.Type}");
    builder.Services.AddDbContext<ExtStoreDbContext>(o =>
        ConfigurationHelper.BuildDbContextOptions(
            o, storageConfig, "MinGo.Identity.Migrations"));
    builder.Services.AddTransient<IDbMigrateService, ExtStoreDbMigrateService>();
    builder.Services.AddTransient<IDataSeeder, InitUserProfileDataSeeder>();

    // 3. Execute migrations + seeders
    var app = builder.Build();
    app.Services.SeedData();
    Console.WriteLine("  ✓ All migrations and seed data applied");
    Console.WriteLine();
    Console.WriteLine("Migration and seeding completed successfully.");
});

scriptCommand.SetHandler(async (FileInfo? output) =>
{
    var config = LoadAppConfiguration();
    var storageConfig = ConfigurationHelper.LoadStorageConfiguration(config);
    var combined = string.Join(
        Environment.NewLine + "-- === Custom Extensions === " + Environment.NewLine,
        GenerateSisScript(storageConfig),
        GenerateExtScript(storageConfig));
    if (output != null)
        await File.WriteAllTextAsync(output.FullName, combined);
    else
        await Console.Out.WriteLineAsync(combined);
}, outputOption);

return await rootCommand.InvokeAsync(args);

static IConfiguration LoadAppConfiguration()
{
    return new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables()
        .Build();
}

static string GenerateSisScript(StorageConfiguration storageConfig)
{
    using var ctx = CreateSisDbContext(storageConfig);
    return ctx.Database.GenerateCreateScript();
}

static string GenerateExtScript(StorageConfiguration storageConfig)
{
    using var ctx = CreateExtDbContext(storageConfig);
    return ctx.Database.GenerateCreateScript();
}

static StoreDbContext CreateSisDbContext(StorageConfiguration storageConfig)
{
    var builder = new DbContextOptionsBuilder<StoreDbContext>();
    ConfigurationHelper.BuildDbContextOptions(
        builder, storageConfig, ConfigurationHelper.GetSisMigrationsAssembly(storageConfig.Type));
    return new StoreDbContext(builder.Options);
}

static ExtStoreDbContext CreateExtDbContext(StorageConfiguration storageConfig)
{
    var builder = new DbContextOptionsBuilder<ExtStoreDbContext>();
    ConfigurationHelper.BuildDbContextOptions(
        builder, storageConfig, "MinGo.Identity.Migrations");
    return new ExtStoreDbContext(builder.Options);
}
