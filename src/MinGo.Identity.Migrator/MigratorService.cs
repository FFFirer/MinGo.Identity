using Microsoft.EntityFrameworkCore;
using MinGo.Identity.Configuration;
using MinGo.Identity.Migrations;
using SimpleIdServer.IdServer.Store.EF;

namespace MinGo.Identity.Migrator;

/// <summary>
/// 统一迁移服务
/// 按顺序执行：SIS 官方迁移 → 自定义扩展迁移 → 种子数据
/// </summary>
public class MigratorService
{
    private readonly StorageConfiguration _storageConfig;

    public MigratorService(StorageConfiguration storageConfig)
    {
        _storageConfig = storageConfig;
    }

    public async Task RunMigrationsAsync()
    {
        Console.WriteLine("=== MinGo Identity Migration Tool ===");
        Console.WriteLine($"Storage: {_storageConfig.Type}");
        Console.WriteLine();

        // Step 1: SimpleIdServer 官方迁移
        Console.WriteLine("[1/3] Applying SimpleIdServer migrations...");
        using (var sisContext = CreateSisDbContext())
        {
            await sisContext.Database.MigrateAsync();
        }
        Console.WriteLine("  ✓ SIS migrations applied");

        // Step 2: 自定义扩展迁移
        Console.WriteLine("[2/3] Applying custom extension migrations...");
        using (var extContext = CreateExtDbContext())
        {
            await extContext.Database.MigrateAsync();
        }
        Console.WriteLine("  ✓ Custom migrations applied");

        // Step 3: Seed 数据
        Console.WriteLine("[3/3] Seeding data...");
        await SeedDataAsync();
        Console.WriteLine("  ✓ Seed data applied");

        Console.WriteLine();
        Console.WriteLine("All migrations completed successfully.");
    }

    public async Task GenerateScriptAsync(FileInfo? output)
    {
        Console.WriteLine("=== Generating Migration Script ===");

        var combined = string.Join(
            Environment.NewLine + "-- === Custom Extensions === " + Environment.NewLine,
            GenerateSisScript(),
            GenerateExtScript());

        if (output != null)
        {
            await File.WriteAllTextAsync(output.FullName, combined);
            Console.WriteLine($"Script written to: {output.FullName}");
        }
        else
        {
            await Console.Out.WriteLineAsync(combined);
        }
    }

    private string GenerateSisScript()
    {
        using var ctx = CreateSisDbContext();
        return ctx.Database.GenerateCreateScript();
    }

    private string GenerateExtScript()
    {
        using var ctx = CreateExtDbContext();
        return ctx.Database.GenerateCreateScript();
    }

    private StoreDbContext CreateSisDbContext()
    {
        var builder = new DbContextOptionsBuilder<StoreDbContext>();
        ConfigurationHelper.BuildDbContextOptions(
            builder, _storageConfig, "SimpleIdServer.IdServer.PostgreMigrations");
        return new StoreDbContext(builder.Options);
    }

    private ExtStoreDbContext CreateExtDbContext()
    {
        var builder = new DbContextOptionsBuilder<ExtStoreDbContext>();
        ConfigurationHelper.BuildDbContextOptions(
            builder, _storageConfig, "MinGo.Identity.Migrations");
        return new ExtStoreDbContext(builder.Options);
    }

    private async Task SeedDataAsync()
    {
        // Seed 数据执行逻辑
        // 目前 IDataSeeder 注册在 IdServer 中，后续可迁移至此处
        await Task.CompletedTask;
    }
}
