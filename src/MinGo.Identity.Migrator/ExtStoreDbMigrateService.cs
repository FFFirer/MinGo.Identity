using DataSeeder;
using Microsoft.EntityFrameworkCore;
using MinGo.Identity.Migrations;

namespace MinGo.Identity.Migrator;

/// <summary>
/// 自定义扩展表迁移服务
/// 包装 <see cref="ExtStoreDbContext.Database.MigrateAsync"/>，使扩展表迁移纳入 SeedData() 统一管理
/// </summary>
public class ExtStoreDbMigrateService : IDbMigrateService
{
    private readonly ExtStoreDbContext _dbContext;

    public ExtStoreDbMigrateService(ExtStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task Migrate(CancellationToken cancellationToken)
        => _dbContext.Database.MigrateAsync(cancellationToken);
}
