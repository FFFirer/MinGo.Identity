using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MinGo.Identity.Configuration;

public static class ConfigurationHelper
{
    /// <summary>
    /// 从 appsettings.json 的 StorageConfiguration 节读取存储配置
    /// </summary>
    public static StorageConfiguration LoadStorageConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(StorageConfiguration));
        return section.Get<StorageConfiguration>() ?? new StorageConfiguration();
    }

    /// <summary>
    /// 统一构建 DbContext 选项，三方项目（IdServer / Migrator / Migrations）调用方式一致
    /// </summary>
    public static DbContextOptionsBuilder BuildDbContextOptions(
        DbContextOptionsBuilder builder,
        StorageConfiguration config,
        string migrationsAssembly)
    {
        switch (config.Type)
        {
            case StorageTypes.SQLSERVER:
                builder.UseSqlServer(config.ConnectionString, o =>
                {
                    o.MigrationsAssembly(migrationsAssembly);
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
                break;

            case StorageTypes.POSTGRE:
                builder.UseNpgsql(config.ConnectionString, o =>
                {
                    o.MigrationsAssembly(migrationsAssembly);
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
                break;

            case StorageTypes.MYSQL:
                builder.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString), o =>
                {
                    o.MigrationsAssembly(migrationsAssembly);
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
                break;

            case StorageTypes.INMEMORY:
                builder.UseInMemoryDatabase(config.ConnectionString);
                break;

            case StorageTypes.SQLITE:
                builder.UseSqlite(config.ConnectionString, o =>
                {
                    o.MigrationsAssembly(migrationsAssembly);
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
                break;
        }

        return builder;
    }
}
