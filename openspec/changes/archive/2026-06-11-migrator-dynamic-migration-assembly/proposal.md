## Why

当前 `MigratorService.cs` 中注册 SimpleIdServer EF Store 时，SIS 和 FormBuilder 的迁移程序集硬编码为 `PostgreMigrations`，导致非 PostgreSQL 存储类型（SQL Server / MySQL / SQLite）下迁移执行失败。而 `SidServerSetup.cs` 已有完整的按存储类型选择迁移程序集的逻辑。需要将这一映射逻辑提取到共享的 `ConfigurationHelper` 中，让 Migrator 和 IdServer 统一复用。

## What Changes

- **`MinGo.Identity.Configuration/ConfigurationHelper.cs`** — 新增 `GetSisMigrationsAssembly()` 和 `GetFormBuilderMigrationsAssembly()` 两个静态方法，按 `StorageTypes` 返回对应的迁移程序集名称
- **`MinGo.Identity.Migrator/MigratorService.cs`** — `SeedDataAsync()` 中调用上述方法替换硬编码字符串；`CreateSisDbContext()` 同步更新
- **`MinGo.IdServer/Conf/SidServerSetup.cs`** — `ConfigureIdserverStorage()` 和 `ConfigureFormbuilderStorage()` 改为调用 `ConfigurationHelper` 的方法，消除重复映射逻辑
- **`MinGo.Identity.Migrator.csproj`** — 无需改动（已有 `Microsoft.EntityFrameworkCore.Sqlite` 等 provider 包引用）

## Capabilities

### New Capabilities

*（无 — 纯内部重构，不引入新的产品能力）*

### Modified Capabilities

*（无 — 运行行为不变，仅迁移程序集选择逻辑从硬编码改为动态分发）*

## Impact

- **MinGo.Identity.Configuration**: 新增两个公开静态方法，不破坏现有 API
- **MinGo.Identity.Migrator**: 行为不变（PostgreSQL 场景），但非 PostgreSQL 场景从「硬编码错误」变为「正确选择对应程序集」
- **MinGo.IdServer**: `ConfigureIdserverStorage()` 和 `ConfigureFormbuilderStorage()` 内部可复用 `ConfigurationHelper` 方法，消除重复代码
