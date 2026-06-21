## ADDED Requirements

### Requirement: Migrator 使用与 IdServer 一致的 DI 容器

Migrator SHALL 通过 `WebApplication.CreateBuilder(args)` + `SidServerSetup.ConfigureIdServer()` 构建 DI 容器，与 IdServer 使用完全一致的服务注册。

Migrator SHALL NOT 使用 `new ServiceCollection()` 或手动注册任何 `IDataSeeder` 或 `IDbMigrateService` 服务。

Migrator SHALL 调用 `app.Services.SeedData()` 执行全部已注册的 `IDbMigrateService` 和 `IDataSeeder`。

Migrator SHALL 在 `SeedData()` 完成后自动退出进程，不调用 `app.Run()`，不监听任何端口。

Migrator SHALL 引用 `MinGo.IdServer` 项目以访问 `SidServerSetup.ConfigureIdServer()`。

#### Scenario: Migrator 运行迁移和种子后退出

- **WHEN** 执行 `dotnet run --project src/MinGo.Identity.Migrator`
- **THEN** 所有 EF Core 迁移被应用
- **THEN** 所有已注册的 `IDataSeeder` 按顺序执行
- **THEN** 进程退出（exit code 0）

#### Scenario: Migrator 不监听 HTTP 端口

- **WHEN** Migrator 启动
- **THEN** 没有端口被绑定
- **THEN** `SeedData()` 完成后进程终止

### Requirement: IdServer 启动时不执行 SeedData

IdServer SHALL NOT 在启动时调用 `app.Services.SeedData()`。

IdServer SHALL 假设数据库已由 Migrator 预先完成迁移和种子。

#### Scenario: IdServer 直连已迁移数据库

- **WHEN** IdServer 启动并收到第一个请求
- **THEN** 数据库表已存在
- **THEN** SIDS-manager 客户端已存在
- **THEN** 不需要任何数据库写操作

### Requirement: docker-compose.dev.yml 集成 Migrator

`docker-compose.dev.yml` SHALL 包含 migrator 服务，作为 idserver 的前置依赖。

migrator 服务 SHALL 在 `postgres` 健康检查通过后启动。

idserver 服务 SHALL 在 migrator 成功完成后才启动（`condition: service_completed_successfully`）。

#### Scenario: 完整开发环境启动

- **WHEN** 执行 `docker compose -f docker/docker-compose.dev.yml up`
- **THEN** postgres 先启动
- **THEN** migrator 启动，执行迁移和种子
- **THEN** migrator 退出（exit code 0）
- **THEN** idserver 启动
- **THEN** admin 启动
