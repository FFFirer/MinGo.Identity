using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MinGo.Identity.Configuration;

namespace MinGo.Identity.Migrations;

/// <summary>
/// 设计时 DbContext 工厂
/// dotnet ef migrations add/remove/list 时自动调用
///
/// 配置来源优先级（从高到低）：
///   1. 命令行参数 --StorageConfiguration:ConnectionString "xxx"
///   2. UserSecrets（本地开发，dotnet user-secrets set）
///   3. appsettings.json（项目目录，可选）
///   4. 默认值（INMEMORY）
/// </summary>
public class DesignTimeFactory : IDesignTimeDbContextFactory<ExtStoreDbContext>
{
    public ExtStoreDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<DesignTimeFactory>(optional: true)
            .AddCommandLine(args ?? [])
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
