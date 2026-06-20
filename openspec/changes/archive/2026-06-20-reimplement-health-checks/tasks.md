## 1. NuGet 包依赖

- [x] 1.1 在 `Directory.Packages.props` 中添加 `Microsoft.Data.SqlClient` 包版本定义（SQL Server 数据库检查）
- [x] 1.2 在 `Directory.Packages.props` 中添加 `StackExchange.Redis` 包版本定义（Redis 键值存储检查）

## 2. HealthCheckExtensions.cs 入口重写

- [x] 2.1 更改 `AddIdentityHealthChecks()` 签名：从 `IServiceCollection` → `IServiceCollection, IConfiguration`，读取配置节并自动注册匹配的健康检查
- [x] 2.2 实现 `MapIdentityHealthChecks(this WebApplication app)` 扩展方法，统一映射 `/healthz`、`/readyz`、`/startupz` 端点
- [x] 2.3 实现 JSON 响应 writer（`HealthCheckResponseWriters.WriteJson`），输出结构化组件级状态
- [x] 2.4 删除旧的 `AddPostgreSqlReadinessCheck()`、`AddIdServerEndpointCheck()`、`HealthCheckRegistrationHelper` 代码

## 3. DatabaseHealthCheck — 多后端数据库检查

- [x] 3.1 创建 `DatabaseHealthCheck : IHealthCheck`，注入 `IConfiguration` 和 `ILogger<DatabaseHealthCheck>`
- [x] 3.2 实现 PostgreSQL 检查：`NpgsqlConnection.OpenAsync()` + `SELECT 1`
- [x] 3.3 实现 SQL Server 检查：`SqlConnection.OpenAsync()` + `SELECT 1`
- [x] 3.4 实现 SQLite 检查：文件访问 + 连接测试
- [x] 3.5 InMemory 类型返回健康

## 4. KeyValueStoreHealthCheck — Redis 检查

- [x] 4.1 创建 `KeyValueStoreHealthCheck : IHealthCheck`，注入 `IConfiguration`
- [x] 4.2 实现 Redis 检查：`StackExchange.Redis.ConnectionMultiplexer` PING
- [x] 4.3 InMemory 类型返回健康

## 5. MessageBrokerHealthCheck — MassTransit 传输检查

- [x] 5.1 创建 `MessageBrokerHealthCheck : IHealthCheck`，注入 `IConfiguration`
- [x] 5.2 实现 SQL Server 传输检查：`SqlConnection.OpenAsync()` + `SELECT 1`
- [x] 5.3 InMemory 类型返回健康

## 6. IdServerEndpointCheck — 重写

- [x] 6.1 重写 `IdServerEndpointCheck : IHealthCheck`，注入 `IHttpClientFactory`、`ILogger<IdServerEndpointCheck>`
- [x] 6.2 实现 OIDC Discovery 端点检查：HTTP GET + 验证 JSON 响应体
- [x] 6.3 实现 SCIM 端点检查（可选，`ScimBaseUrl` 非空时注册）

## 7. IdServer 集成

- [x] 7.1 更新 `SidServerSetup.cs`：用 `AddIdentityHealthChecks(configuration)` 替代现有手动注册代码
- [x] 7.2 更新 `IdServer/Program.cs`：用 `MapIdentityHealthChecks()` 替代三个手写 `MapHealthChecks` 调用

## 8. Admin 集成

- [x] 8.1 更新 `Admin/Program.cs`：用 `AddIdentityHealthChecks(configuration)` 替代现有注册代码
- [x] 8.2 更新 `Admin/Program.cs`：用 `MapIdentityHealthChecks()` 替代三个手写 `MapHealthChecks` 调用
- [x] 8.3 清理 Admin/Program.cs 中的注释死代码块（`ForwardedHeaders`、`UseForwardedHeaders`、注释掉的 `AddHealthChecks`）

## 9. 验证

- [x] 9.1 `lsp_diagnostics` 检查 Configuration 项目 — 0 错误
- [x] 9.2 `dotnet build` IdServer — 构建成功
- [x] 9.3 `dotnet build` Admin — 构建成功
- [x] 9.4 代码审查：确认所有检查通过 DI 解析，无 `new HttpClient`、无 `ActivatorUtilities.CreateInstance`
