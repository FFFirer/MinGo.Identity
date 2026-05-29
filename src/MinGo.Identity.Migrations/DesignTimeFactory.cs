using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MinGo.Identity.Configuration;

namespace MinGo.Identity.Migrations;

/// <summary>
/// 设计时 DbContext 工厂
/// dotnet ef migrations add 时会自动调用此工厂
/// </summary>
public class DesignTimeFactory : IDesignTimeDbContextFactory<ExtStoreDbContext>
{
    public ExtStoreDbContext CreateDbContext(string[] args)
    {
        // 尝试从 appsettings.json 读取配置（设计时在 Migrations 项目目录执行）
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var storageConfig = ConfigurationHelper.LoadStorageConfiguration(config);

        var builder = new DbContextOptionsBuilder<ExtStoreDbContext>();
        ConfigurationHelper.BuildDbContextOptions(
            builder,
            storageConfig,
            "MinGo.Identity.Migrations");

        return new ExtStoreDbContext(builder.Options);
    }
}
