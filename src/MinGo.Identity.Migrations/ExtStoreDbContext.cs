using Microsoft.EntityFrameworkCore;

namespace MinGo.Identity.Migrations;

/// <summary>
/// 自定义扩展表 DbContext
/// 用于管理 SimpleIdServer 官方表之外的业务扩展表
/// </summary>
public class ExtStoreDbContext : DbContext
{
    public ExtStoreDbContext(DbContextOptions<ExtStoreDbContext> options) : base(options) { }

    /// <summary>用户扩展信息</summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
        });
    }
}

/// <summary>用户扩展信息（示例实体）</summary>
public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Subject { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
