using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MinGo.Identity.Configuration;

public static class ConfigurationHelper
{
    /// <summary>
    /// 根据存储类型返回 SimpleIdServer IdServer 对应的迁移程序集名称
    /// </summary>
    public static string? GetSisMigrationsAssembly(StorageTypes type) => type switch
    {
        StorageTypes.SQLSERVER => "SimpleIdServer.IdServer.SqlServerMigrations",
        StorageTypes.POSTGRE => "SimpleIdServer.IdServer.PostgreMigrations",
        StorageTypes.MYSQL => throw new NotSupportedException("MySQL storage is not supported. Use PostgreSQL instead."),
        // StorageTypes.MYSQL => "SimpleIdServer.IdServer.MySQLMigrations",
        StorageTypes.SQLITE => "SimpleIdServer.IdServer.SqliteMigrations",
        StorageTypes.INMEMORY => null,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    /// <summary>
    /// 根据存储类型返回 FormBuilder 对应的迁移程序集名称
    /// </summary>
    public static string? GetFormBuilderMigrationsAssembly(StorageTypes type) => type switch
    {
        StorageTypes.SQLSERVER => "SidFormBuilder.SqlServerMigrations",
        StorageTypes.POSTGRE => "SidFormBuilder.PostgreMigrations",
        StorageTypes.MYSQL => throw new NotSupportedException("MySQL storage is not supported. Use PostgreSQL instead."),
        // StorageTypes.MYSQL => "SidFormBuilder.MySQLMigrations",
        StorageTypes.SQLITE => "SidFormBuilder.SqliteMigrations",
        StorageTypes.INMEMORY => null,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

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
        string? migrationsAssembly)
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
                throw new NotSupportedException("MySQL storage is not supported. Use PostgreSQL instead.");
                // builder.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString), o =>
                // {
                //     o.MigrationsAssembly(migrationsAssembly);
                //     o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                // });
                // break;

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
