## Context

当前 Migrator 和 IdServer 各自维护了迁移程序集的映射逻辑：

| 位置 | 实现方式 | 问题 |
|------|---------|------|
| `SidServerSetup.cs` `ConfigureIdserverStorage()` | switch 按 `StorageTypes` 选择程序集 | 正确的逻辑，但与 Migrator 重复 |
| `SidServerSetup.cs` `ConfigureFormbuilderStorage()` | switch 按 `StorageTypes` 选择程序集 | 同上 |
| `MigratorService.cs` `SeedDataAsync()` | 硬编码 `"SimpleIdServer.IdServer.PostgreMigrations"` | 仅支持 PostgreSQL |
| `MigratorService.cs` `SeedDataAsync()` | 硬编码 `"SidFormBuilder.PostgreMigrations"` | 仅支持 PostgreSQL |

`ConfigurationHelper.BuildDbContextOptions()` 已统一处理所有 provider 的 DbContext 配置差异，唯独 `migrationsAssembly` 参数在各调用点硬编码。

## Goals / Non-Goals

**Goals:**
- `ConfigurationHelper` 新增 `GetSisMigrationsAssembly()` 和 `GetFormBuilderMigrationsAssembly()` 两个静态方法
- `MigratorService` 中 SIS / FormBuilder / Ext 三处的 migrationsAssembly 均按 `StorageTypes` 动态选择
- `SidServerSetup.cs` 中 `ConfigureIdserverStorage()` 和 `ConfigureFormbuilderStorage()` 复用 `ConfigurationHelper` 方法
- 已有 `ConfigurationHelper.BuildDbContextOptions()` 签名不变（`migrationsAssembly` 参数保留，调用方传动态值）

**Non-Goals:**
- 不改动 `ConfigurationHelper.BuildDbContextOptions()` 的内部实现
- 不改动 `StorageTypes` 枚举或 `StorageConfiguration` 模型
- 不涉及 `ExtStoreDbContext` 的迁移程序集（其 `"MinGo.Identity.Migrations"` 是固定的，无需按存储类型变化）

## Decisions

### 决策 1：方法放在 `ConfigurationHelper`（C）而非 `MigratorService`（A）或两者各一份（B）

| 方案 | 评估 |
|------|------|
| **C. `ConfigurationHelper` 集中管理** | 单一真相来源，IdServer 和 Migrator 都能调用。符合「复用相同逻辑」的要求 ✅ |
| A. `MigratorService` 私有方法 | IdServer 无法复用，重复逻辑仍然存在 ❌ |
| B. 两者各维护一份 | 重复 + 维护负担，与目标相反 ❌ |

**选择**：方案 C，`ConfigurationHelper` 新增两个方法。

### 决策 2：返回 `string?` 而非 `string`（INMEMORY 返回 null）

`StorageTypes.INMEMORY` 不需要迁移程序集（`UseInMemoryDatabase` 不涉及迁移）。由调用方在得到 null 时跳过 migrations assembly 配置。这与 `BuildDbContextOptions` 当前行为一致——INMEMORY 分支不调用 `o.MigrationsAssembly()`。

### 决策 3：SidServerSetup 的 `ConfigureIdserverStorage()` 改为委托给 `ConfigurationHelper`

```csharp
// Before (SidServerSetup.cs)
b.UseNpgsql(conf.ConnectionString, o => {
    o.MigrationsAssembly("SimpleIdServer.IdServer.PostgreMigrations");
    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
});

// After
ConfigurationHelper.BuildDbContextOptions(b, conf,
    ConfigurationHelper.GetSisMigrationsAssembly(conf.Type));
```

迁移程序集选择 + provider 配置统一由 `ConfigurationHelper` 管理，`SidServerSetup` 只负责读配置节。

### 决策 4：SQLite 的 `AmbientTransactionWarning` 也纳入 `ConfigurationHelper`

`ConfigureIdserverStorage` 和 `ConfigureFormbuilderStorage` 中 SQLite 分支有 `b.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning))`。当前 `ConfigurationHelper.BuildDbContextOptions()` 没有这个设置。这是一个功能差距，但：

- 该 warning 是运行时行为，不影响迁移执行
- 迁移场景（Migrator）不涉及事务警告
- IdServer 运行时（SidServerSetup）保留该设置即可

**决定**：本次不将 `AmbientTransactionWarning` 纳入 `ConfigurationHelper`，后续单独处理。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| `GetSisMigrationsAssembly` 返回 null 时调用方忘记处理 | `BuildDbContextOptions` 已有 INMEMORY 分支，null 传入 migrationsAssembly 参数在该分支不生效，安全 |
| 新方法上线后，Migrator 配置为 PostgreSQL 的行为不变 | 回归验证：`dotnet run -- migrate` 与当前输出一致 |
| `SidServerSetup` 改为委托后，SQLite AmbientTransactionWarning 丢失 | 已在决策 4 确认：当前 `BuildDbContextOptions` 未涵盖，SidServerSetup 需在调用后补加 |
