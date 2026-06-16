## 1. NuGet 包配置

- [x] 1.1 在 `Directory.Packages.props` 中添加 `Microsoft.Extensions.Diagnostics.HealthChecks` 和 `Abstractions` 包版本定义

## 2. 共享扩展方法

- [x] 2.1 在 `src/MinGo.Identity.Configuration/` 中创建 `HealthCheckExtensions.cs`，包含 `AddIdentityHealthChecks()`、`AddPostgreSqlReadinessCheck()`、`AddIdServerEndpointCheck()` 扩展方法
- [x] 2.2 实现 `PostgreSqlHealthCheck`（自定义 `IHealthCheck`，使用 `NpgsqlConnection` 验证 PG 可达性）
- [x] 2.3 实现 `IdServerHealthCheck`（自定义 `IHealthCheck`，HTTP GET 检查 IdServer OIDC 端点）

## 3. IdServer 集成

- [x] 3.1 在 `SidServerSetup.cs` 末尾调用 `AddIdentityHealthChecks()` + `AddPostgreSqlReadinessCheck()` 注册健康检查
- [x] 3.2 在 `IdServer/Program.cs` 中（`app.Build()` 之后，其他中间件之前）映射 `/healthz`、`/readyz`、`/startupz` 端点

## 4. Admin 集成

- [x] 4.1 在 `Admin/Program.cs` 中调用 `AddIdentityHealthChecks()` + `AddIdServerEndpointCheck()` 注册健康检查
- [x] 4.2 在 `Admin/Program.cs` 中（`app.Build()` 之后）映射 `/healthz`、`/readyz`、`/startupz` 端点

## 5. Docker 配置更新

- [x] 5.1 从 `Dockerfile.idserver` 中移除 `HEALTHCHECK` 指令
- [x] 5.2 从 `Dockerfile.admin` 中移除 `HEALTHCHECK` 指令
- [x] 5.3 更新 `docker-compose.prod.yml` 中的 IdServer healthcheck 指向 `http://localhost:5001/readyz`
- [x] 5.4 更新 `docker-compose.prod.yml` 中的 Admin healthcheck 指向 `http://localhost:5002/readyz`
- [x] 5.5 更新 `docker-compose.prod.yml` 中 Admin 的 `depends_on.idserver.condition` 为 `service_healthy`

## 6. 验证

- [x] 6.1 `lsp_diagnostics` 检查 Configuration 和 Admin 项目 0 错误 | IdServer 错误均为 SimpleIdServer 包还原超时（预存问题）
- [x] 6.2 `dotnet build` — Configuration ✅ |  Admin ✅ | IdServer 预存包还原失败
