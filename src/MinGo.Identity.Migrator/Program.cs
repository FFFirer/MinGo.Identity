using System.CommandLine;
using Microsoft.Extensions.Configuration;
using MinGo.Identity.Configuration;
using MinGo.Identity.Migrator;

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
    var config = LoadAppConfiguration();
    var storageConfig = ConfigurationHelper.LoadStorageConfiguration(config);
    var service = new MigratorService(storageConfig);
    await service.RunMigrationsAsync();
});

scriptCommand.SetHandler(async (FileInfo? output) =>
{
    var config = LoadAppConfiguration();
    var storageConfig = ConfigurationHelper.LoadStorageConfiguration(config);
    var service = new MigratorService(storageConfig);
    await service.GenerateScriptAsync(output);
}, outputOption);

return await rootCommand.InvokeAsync(args);

static IConfiguration LoadAppConfiguration()
{
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables()
        .Build();
}
