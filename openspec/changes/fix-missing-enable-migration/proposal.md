## Why

IdServer 启动时因 DI 验证失败崩溃：`LaunchMigrationConsumer` 依赖于 `ILaunchMigrationService`，但该服务从未注册。根因是在从 SimpleIdServer 模板生成后，重构迁移逻辑时遗漏了 `.EnableMigration()` 调用。

## What Changes

- 在 `SidServerSetup.cs` 的 `IdServerBuilder` 链式调用中添加 `.EnableMigration()` 方法调用
- 注册 `ILaunchMigrationService`、`ILaunchAllMigrationsService`、`IMigrationServiceFactory` 及相关路由
- 无新增 NuGet 包引用（`SimpleIdServer.IdServer.Migrations` 已通过传递依赖可用）

## Capabilities

### New Capabilities

无——本次变更仅为实现层面的修复，不引入新能力。

### Modified Capabilities

- `idserver-core`: 修复 SidServerSetup 配置链，补全模板应包含的 `.EnableMigration()` 调用

## Impact

- **唯一变更文件**: `src/MinGo.IdServer/Conf/SidServerSetup.cs`
- 新增 1 行代码到 builder 链
- 无新增依赖，无配置变更，无 API 变更
- 修复后 `ILaunchMigrationService` 等迁移相关服务可正常注入
