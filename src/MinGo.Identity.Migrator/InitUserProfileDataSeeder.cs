using DataSeeder;
using Microsoft.EntityFrameworkCore;
using MinGo.Identity.Migrations;

namespace MinGo.Identity.Migrator;

/// <summary>
/// 自定义扩展表种子数据
/// 遵循 SimpleIdServer 标准：继承 <see cref="BaseAfterDeploymentDataSeeder"/>，自动获得幂等跟踪
/// </summary>
public class InitUserProfileDataSeeder : BaseAfterDeploymentDataSeeder
{
    private readonly ExtStoreDbContext _dbContext;

    public InitUserProfileDataSeeder(
        ExtStoreDbContext dbContext,
        IDataSeederExecutionHistoryRepository dataSeederExecutionHistoryRepository)
        : base(dataSeederExecutionHistoryRepository)
    {
        _dbContext = dbContext;
    }

    public override string Name => nameof(InitUserProfileDataSeeder);

    protected override async Task Execute(CancellationToken cancellationToken)
    {
        if (await _dbContext.UserProfiles.AnyAsync(cancellationToken))
            return;

        // 插入示例管理员用户扩展信息
        _dbContext.UserProfiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            Subject = "administrator",
            DisplayName = "Administrator",
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
