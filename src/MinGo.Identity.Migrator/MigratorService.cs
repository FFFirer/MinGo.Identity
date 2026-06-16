using DataSeeder;
using FormBuilder;
using FormBuilder.EF;
using FormBuilder.EF.Stores;
using FormBuilder.Repositories;
using FormBuilder.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinGo.Identity.Configuration;
using MinGo.Identity.Migrations;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.Store.EF;
using SimpleIdServer.IdServer.Migrations;
using SimpleIdServer.IdServer.Migrations.DataSeeder;
using SimpleIdServer.IdServer.Stores;

namespace MinGo.Identity.Migrator;

/// <summary>
/// 统一迁移服务
/// 通过 SimpleIdServer 官方的 SeedData() pipeline 统一管理：
///   - 数据库迁移（SIS + FormBuilder + 自定义扩展）
///   - 种子数据（SIS 14 个官方 seeder + 自定义扩展 seeder）
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

        Console.WriteLine("[1/1] Running SeedData pipeline (migrations + seeders)...");
        await SeedDataAsync();
        Console.WriteLine("  ✓ All migrations and seed data applied");

        Console.WriteLine();
        Console.WriteLine("All operations completed successfully.");
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

    /// <summary>
    /// 构建 DI 容器，注册所有 IDataSeeder 和 IDbMigrateService，
    /// 然后调用 SimpleIdServer 官方的 SeedData() 扩展方法执行全部 pipeline。
    /// </summary>
    private async Task SeedDataAsync()
    {
        var services = new ServiceCollection();

        EnableMigration(services);

        // ════════════════════════════════════════════════════════════════
        // SIS EF Store: StoreDbContext + EF 仓库 + IDbMigrateService x2
        //   (IdServerDbMigrateService + FormbuilderDbMigrateService)
        // ════════════════════════════════════════════════════════════════
        services.AddEfStore(o =>
            ConfigurationHelper.BuildDbContextOptions(
                o, _storageConfig, ConfigurationHelper.GetSisMigrationsAssembly(_storageConfig.Type)));

        // ════════════════════════════════════════════════════════════════
        // FormBuilder EF Store: IFormStore + FormBuilderDbContext
        //   InitConsoleAuthDataseeder 等 seeder 依赖 IFormStore
        // ════════════════════════════════════════════════════════════════
        services.AddTransient<IFormStore, FormStore>();
        services.AddTransient<IWorkflowStore, WorkflowStore>();
        services.AddTransient<ITemplateStore, TemplateStore>();
        services.AddDbContext<FormBuilderDbContext>(o =>
            ConfigurationHelper.BuildDbContextOptions(
                o, _storageConfig, ConfigurationHelper.GetFormBuilderMigrationsAssembly(_storageConfig.Type)));

        // ════════════════════════════════════════════════════════════════
        // 自定义扩展 EF Store
        // ════════════════════════════════════════════════════════════════
        services.AddDbContext<ExtStoreDbContext>(o =>
        {
            ConfigurationHelper.BuildDbContextOptions(
                o, _storageConfig, "MinGo.Identity.Migrations");
            // 模型快照基于 Npgsql 生成，SQLite 等 provider 类型映射不同会导致 PendingModelChangesWarning
            o.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // ════════════════════════════════════════════════════════════════
        // 数据迁移服务
        // ════════════════════════════════════════════════════════════════
        services.AddDataseeder();

        // ════════════════════════════════════════════════════════════════
        // SimpleIdServer 官方 14 个 IDataSeeder
        // ════════════════════════════════════════════════════════════════
        RegisterSisDataSeeders(services);

        // ════════════════════════════════════════════════════════════════
        // SimpleIdServer SeedAdministrationData — 管理后台种子数据
        // 等价于 IdServerBuilder.SeedAdministrationData()，创建 SIDS-manager 客户端等
        // ════════════════════════════════════════════════════════════════
        RegisterAdministrationSeeders(services);

        // ════════════════════════════════════════════════════════════════
        // 自定义扩展
        // ════════════════════════════════════════════════════════════════
        services.AddTransient<IDbMigrateService, ExtStoreDbMigrateService>();
        services.AddTransient<IDataSeeder, InitUserProfileDataSeeder>();

        // ════════════════════════════════════════════════════════════════
        // SIS EnableMigration — 注册 IMigrationServiceFactory、
        // AddMigrationsScopeDataSeeder 等（等价于 IdServer 的 .EnableMigration()）
        // ════════════════════════════════════════════════════════════════
        EnableMigration(services);

        var sp = services.BuildServiceProvider();
        sp.SeedData();
    }

    /// <summary>
    /// 注册 SimpleIdServer SeedAdministrationData 中定义的种子数据 seeder。
    /// 等价于 IdServerBuilder.SeedAdministrationData() 的 DI 注册逻辑。
    /// 创建 SIDS-manager 客户端、管理域、用户组和种子用户。
    /// </summary>
    private static void RegisterAdministrationSeeders(IServiceCollection services)
    {
        // 管理域
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitAdministrativeScopeDataSeeder>();
        // 用户组（administrators 等）
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitGroupDataSeeder>();
        // 种子用户（SIS 默认用户）
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitUserDataSeeder>();
        // SIDS-manager 管理客户端 — 这是 admin 容器 OIDC client_credentials 所依赖的核心
        services.AddTransient<IDataSeeder>((s) =>
        {
            var scope = s.CreateScope();
            var transactionBuilder = scope.ServiceProvider.GetRequiredService<ITransactionBuilder>();
            var realmRepository = scope.ServiceProvider.GetRequiredService<IRealmRepository>();
            var scopeRepository = scope.ServiceProvider.GetRequiredService<IScopeRepository>();
            var clientRepository = scope.ServiceProvider.GetRequiredService<IClientRepository>();
            var dataseederRepository = scope.ServiceProvider.GetRequiredService<IDataSeederExecutionHistoryRepository>();
            return new SimpleIdServer.IdServer.DataSeeder.InitAdministrativeClientDataseeder(
                new List<string>(),         // redirectUrls — 由 IdServer 启动时的 AfterDeployment 补充
                new List<string>(),         // postLogoutUrls
                string.Empty,               // backchannelLogoutUrl
                new List<Scope>(),          // additionalScopes
                transactionBuilder,
                realmRepository,
                scopeRepository,
                clientRepository,
                dataseederRepository);
        });
        // 为 SIDS-manager 分配 Templates scope
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.AssignTemplateScopeToClientDataSeeder>();
    }

    private void EnableMigration(ServiceCollection services)
    {
        services.AddTransient<IMigrationServiceFactory, MigrationServiceFactory>();
        services.AddTransient<IDataSeeder, AddMigrationsScopeDataSeeder>();
        services.AddTransient<ILaunchMigrationService, LaunchMigrationService>();
        services.AddTransient<ILaunchAllMigrationsService, LaunchAllMigrationsService>();
    }


    /// <summary>
    /// 注册 SimpleIdServer 官方 ConfigureDataSeeders 中定义的 14 个 IDataSeeder。
    /// AddMigrationsScopeDataSeeder 由 <see cref="EnableMigration"/> 单独注册。
    /// </summary>
    private static void RegisterSisDataSeeders(IServiceCollection services)
    {
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitRealmDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitSerializedFileKeyDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitCertificateAuthoritiesDataseeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitLanguageDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitScopeDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitConsoleAuthDataseeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitUserLockingConfigurationDefDataseeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.InitConsoleConfigurationDefDataseeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.UpdateAuthorizationCodeLifetimeDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.UpdateDeviceCodeLitetimeDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.UpdateParLitetimeDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.UpdateOtherLifetimeDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.UpdateClientSecretDataSeeder>();
        services.AddTransient<IDataSeeder, SimpleIdServer.IdServer.DataSeeder.UpdateTargetsConsoleWorkflowsDataseeder>();
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
            builder, _storageConfig, ConfigurationHelper.GetSisMigrationsAssembly(_storageConfig.Type));
        return new StoreDbContext(builder.Options);
    }

    private ExtStoreDbContext CreateExtDbContext()
    {
        var builder = new DbContextOptionsBuilder<ExtStoreDbContext>();
        ConfigurationHelper.BuildDbContextOptions(
            builder, _storageConfig, "MinGo.Identity.Migrations");
        return new ExtStoreDbContext(builder.Options);
    }
}
