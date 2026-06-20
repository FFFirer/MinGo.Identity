## Why

当前健康检查实现存在 Bug（SQL Server 存储类型下误用 Npgsql 检查）、资源泄漏（HttpClient 不 Dispose）、架构问题（MapIdentityHealthChecks() 未实现导致端点映射重复、检查注册不完整）。生产部署在即，需要一套覆盖所有外部依赖、可维护、可扩展的健康检查方案。

## What Changes

- **重写 `AddIdentityHealthChecks()`**：从空壳改为接收 `IConfiguration`，自动读取所有配置节（Storage / KeyValue / MessageBroker / DistributedCache），按实际启用的后端注册对应的健康检查
- **实现 `MapIdentityHealthChecks()`**：将 IdServer 和 Admin 中重复的 3 个端点映射提取到共享库
- **增加数据库检查支持全部存储类型**：PostgreSQL / SQL Server / SQLite（当前仅支持 PostgreSQL，且 SQL Server 下会运行时崩溃）
- **增加键值存储检查**：Redis 连通性（如果配置了 Redis）
- **增加消息传输检查**：MassTransit SQL Server 传输连通性（如果使用外部传输）
- **所有检查通过 DI 解析**：不再手动 `new HttpClient`，改用 `IHttpClientFactory` + `ILogger<T>`
- **结构化 JSON 响应**：自定义 ResponseWriter，输出组件级状态详情
- **修复现有 Bug**：SQL Server 条件判断错误、HttpClient 泄漏
- **清理 Admin 遗留代码**：已注释的死代码块
- 不改变端点路径（`/healthz` / `/readyz` / `/startupz`）
- 不改变 Docker HEALTHCHECK 配置

## Capabilities

### Modified Capabilities
- `health-checks`: 扩展就绪检查范围——从仅 PostgreSQL 扩展到全部存储类型（SQL Server/SQLite），增加键值存储（Redis）和消息传输（SQL Server）检查。注册方式从手动改为配置驱动。响应格式从纯文本改为结构化 JSON。

## Impact

- `src/MinGo.Identity.Configuration/HealthCheckExtensions.cs`：整体重写，新增 `MapIdentityHealthChecks()`，更改 `AddIdentityHealthChecks()` 签名
- `src/MinGo.IdServer/Conf/SidServerSetup.cs`：健康检查注册段重写（约 10 行）
- `src/MinGo.IdServer/Program.cs`：约 15 行端点映射代码替换为一行 `MapIdentityHealthChecks()`
- `src/MinGo.IdServer.Admin/Program.cs`：同上 + 清理注释死代码
- `Directory.Packages.props`：可能需要新增 `Microsoft.Data.SqlClient` 包（SQL Server 检查用）
