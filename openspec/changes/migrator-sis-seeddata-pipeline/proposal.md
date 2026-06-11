## Why

运行时异常 `The default language en is not present in the repository. Please add it` 源于 IdServer 删除了 `app.Services.SeedData()` 调用，导致 SimpleIdServer 的 13 个官方 `IDataSeeder`（含 `InitLanguageDataSeeder`）从未执行，Language 表为空。当前 Migrator 只做 `Database.MigrateAsync()` 建表，`SeedDataAsync()` 是空方法。

需要将 Migrator 的种子数据机制改造为完全遵循 SimpleIdServer 官方标准——通过 `SeedData()` 统一管理数据库迁移和种子数据，同时将自定义扩展表 `ExtStoreDbContext` 也纳入同一 pipeline。

## What Changes

- **MigratorService.cs** — 重写 `SeedDataAsync()`：构建 DI 容器 → 注册 SIS 和 Ext 的 EF Store、IDataSeeder、IDbMigrateService → 调用 `sp.SeedData()`
- **ExtStoreDbMigrateService.cs** (新建) — 实现 `IDbMigrateService`，包装 `ExtStoreDbContext.Database.MigrateAsync()`
- **InitUserProfileDataSeeder.cs** (新建) — 实现 `IDataSeeder`，为 `user_profiles` 表提供种子数据
- **MinGo.Identity.Migrator.csproj** — 添加 `SimpleIdServer.IdServer` NuGet 引用
- **移除**原 `RunMigrationsAsync()` 的三步独立流程，统一由 `SeedData()` 管理

## Capabilities

### New Capabilities

- `sis-seeddata-pipeline`: Migrator 通过 SimpleIdServer 官方 `SeedData()` 扩展方法统一执行数据库迁移 + 种子数据
- `ext-store-seeddata`: 自定义扩展表 `ExtStoreDbContext` 通过 `IDbMigrateService` + `IDataSeeder` 纳入标准 pipeline

### Modified Capabilities

- *(无 — 不涉及 spec 级行为变更)*

## Impact

- **MinGo.Identity.Migrator**: `RunMigrationsAsync()` 流程重构，三步合并为一步通过 `SeedData()` 管理。需新增 `SimpleIdServer.IdServer` 包引用
- **MinGo.IdServer**: `Program.cs` 保持现状（不移回 `SeedData()`），种子数据完全由 Migrator 负责
- **MinGo.Identity.Migrations** (ExtStoreDbContext): 新增 `IDbMigrateService` 和 `IDataSeeder` 实现，迁移 + 种子纳入统一管理
- **数据库**: `DataSeederExecutionHistory` 表自动跟踪所有 seeder 执行，后续运行幂等安全
