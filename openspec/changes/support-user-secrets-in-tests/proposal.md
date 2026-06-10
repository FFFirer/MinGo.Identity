## Why

当前集成测试中 DbFixture 和 CreateMigratorService() 各自独立读取 appsettings.json，导致环境变量或 UserSecrets 覆盖的连接串只影响数据库生命周期，不影响迁移执行。需要统一测试配置源并支持 UserSecrets，方便开发者在本地开发时替换数据库连接而不必修改 appsettings.json。

## What Changes

- `MinGo.Identity.Migrator.IntegrationTests.csproj`：添加 `<UserSecretsId>` 和 `Microsoft.Extensions.Configuration.UserSecrets` 包引用
- `DbFixture.cs`：配置管道增加 `.AddUserSecrets<>()`，解析后暴露 `StorageConfiguration` 属性
- `MigratorIntegrationTests.cs`：`CreateMigratorService()` 不再独立读取 appsettings.json，改为复用 `DbFixture.StorageConfiguration`

## Capabilities

### New Capabilities

- `integration-test-config`: 集成测试配置管理，支持通过 UserSecrets、环境变量、appsettings.json 三级配置覆盖测试数据库连接

### Modified Capabilities

<!-- No existing spec changes needed — this is purely test infrastructure improvement -->

## Impact

- 仅涉及测试项目代码（3 个文件）
- 不涉及生产代码变更
- 不涉及依赖或环境变更
- 开发者可通过 `dotnet user-secrets set` 覆盖连接串
