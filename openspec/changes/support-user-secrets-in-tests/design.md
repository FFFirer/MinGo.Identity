## Context

集成测试项目 `MinGo.Identity.Migrator.IntegrationTests` 当前配置管理有两个问题：

1. **配置解析重复**：`DbFixture` 和 `CreateMigratorService()` 各自独立调用 `new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()`，产生两套配置实例
2. **配置覆盖不同步**：`DbFixture` 支持 `MINGO_TEST_CONNECTION_STRING` 环境变量覆盖，但 `CreateMigratorService()` 读取的是原始 appsettings.json 值
3. **缺少 UserSecrets 支持**：其他项目（Migrations、IdServer）已使用 UserSecrets，但测试项目没有

生产代码 `MigratorService` 已正确设计——它只接受 `StorageConfiguration` 参数，对配置来源零知识。问题仅在测试基础设施内部。

## Goals / Non-Goals

**Goals:**
- 测试项目支持 `dotnet user-secrets` 配置覆盖
- `DbFixture` 和 `CreateMigratorService()` 使用同一配置源
- 配置文件仅解析一次，后续复用
- 环境变量 `MINGO_TEST_CONNECTION_STRING` 保留且优先级最高

**Non-Goals:**
- 不修改生产代码（`MigratorService`、`ConfigurationHelper` 等）
- 不修改 appsettings.json 的默认值（`mingo_identity_test` 仍是默认库）
- 不改变 CLI 端到端测试的行为（`Cli_Migrate_ShouldSucceed` 使用 Migrator 自身配置）
- 不引入测试配置的抽象层或依赖注入容器

## Decisions

### 1. UserSecretsId：测试项目独立持有

| 方案 | 评价 |
|------|------|
| 测试项目自持 `mingo-identity-tests-2026` | ✅ 解耦，自包含 |
| 复用 Migrations 的 `mingo-identity-migrations-2026` | ❌ 跨项目耦合，`AddUserSecrets<T>()` 需引用 Migrations 类型 |

**结论**：测试项目拥有自己的 UserSecretsId，通过 `AddUserSecrets<DbFixture>()` 引用。

### 2. 配置优先级

```
1. 环境变量 MINGO_TEST_CONNECTION_STRING   ← CI/CD / 临时覆盖
2. UserSecrets                              ← 本地开发 dotnet user-secrets set
3. appsettings.json                         ← 版本控制的默认值
```

### 3. DbFixture 暴露 StorageConfiguration

`DbFixture` 在构造函数中解析配置后，以只读属性 `StorageConfiguration` 暴露，供 `CreateMigratorService()` 复用。

### 4. 环境变量保持手动读取

不使用 `AddEnvironmentVariables()`，而是继续手动 `Environment.GetEnvironmentVariable("MINGO_TEST_CONNECTION_STRING")`。原因是环境变量键名使用 `MINGO_TEST_` 前缀比 .NET 配置系统的 `StorageConfiguration__ConnectionString` 双下划线语法更直观。

## Risks / Trade-offs

- **配置变更在测试运行中不生效**：`DbFixture` 在 xUnit 测试类构造函数中解析一次配置，运行期间修改 env var 或 UserSecrets 不会影响已启动的测试（这是预期行为，重新运行即可）
- **xUnit fixture 生命周期**：`IClassFixture<DbFixture>` 确保所有测试共享同一 DbFixture 实例，配置只解析一次
