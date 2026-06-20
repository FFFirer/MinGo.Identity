## Context

MinGo.Identity 包含两个 ASP.NET Core 服务：IdServer（OIDC/OAuth2 提供者）和 Admin（Blazor 管理面板）。当前健康检查实现在 `MinGo.Identity.Configuration` 项目中，存在以下问题：

- `AddIdentityHealthChecks()` 是空壳，只转发到 `services.AddHealthChecks()`
- `MapIdentityHealthChecks()` 从未实现，端点映射在 IdServer 和 Admin 中重复
- 自定义 `PostgreSqlHealthCheck` 和 `IdServerHealthCheck` 通过 `ActivatorUtilities.CreateInstance` 手动构造，不使用 DI
- `IdServerHealthCheck` 内部 `new HttpClient` 不 Dispose
- SidServerSetup 中条件判断错误：SQL Server 存储类型下调用了 `AddPostgreSqlReadinessCheck`（内部用 NpgsqlConnection），运行时崩溃
- 只覆盖 PostgreSQL 一种数据库，不覆盖 SQL Server/SQLite
- 不覆盖 Redis 键值存储、MassTransit 消息传输
- 响应格式为纯文本，无组件级详情

## Goals / Non-Goals

**Goals:**
- `AddIdentityHealthChecks(IConfiguration)` 接收配置对象，自动读取所有配置节，按实际启用的后端注册健康检查
- 实现 `MapIdentityHealthChecks(this WebApplication app)` 统一映射三个端点
- IdServer 检查：数据库（全部存储类型）、键值存储（Redis）、消息传输（SQL Server MassTransit）
- Admin 检查：IdServer OIDC 端点、SCIM 端点（可选）
- 所有检查通过 DI 解析（`IHttpClientFactory`、`ILogger<T>`、`IOptions<T>`）
- 结构化 JSON 响应：输出每个组件的独立状态、耗时、描述
- 修复现有 Bug（SQL Server 条件判断、HttpClient 泄漏）
- 清理 Admin 中的注释死代码

**Non-Goals:**
- 不添加健康检查 UI 仪表盘（AspNetCore.HealthChecks.UI）
- 不添加 Prometheus 指标导出
- 不改变端点路径（/healthz / /readyz / /startupz）
- 不改变 Docker HEALTHCHECK 配置
- 不修改 SimpleIdServer 框架代码
- 不添加鉴权（健康检查内部端点，匿名访问）
- 不实现分布式缓存健康检查（延后，仅影响性能）

## Decisions

### 1. AddIdentityHealthChecks 签名变更

```csharp
// 旧 (空壳)
public static IHealthChecksBuilder AddIdentityHealthChecks(this IServiceCollection services)
    => services.AddHealthChecks();

// 新 (配置驱动)
public static IServiceCollection AddIdentityHealthChecks(
    this IServiceCollection services,
    IConfiguration configuration)
```

**理由**：将配置读取集中到注册入口，调用者只需传入 `IConfiguration`，内部自动解析各配置节并注册对应检查。消除调用方手写条件判断的需要。

**替代方案考虑**：让每个 `Program.cs` 各自读取配置并注册检查。否决理由——这正是当前重复代码的根源，且新增检查类型时需要改两个地方。

### 2. 放弃 AddTypeActivatedHealthCheck 模式，改为 DI 优先

```csharp
// 旧
services.AddHealthChecks()
    .AddTypeActivatedHealthCheck<PostgreSqlHealthCheck>("postgresql", ..., connectionString);

// 新
services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy, ["ready"]);
```

`DatabaseHealthCheck` 通过构造函数注入 `IOptions<StorageConfiguration>` 和 `ILogger<DatabaseHealthCheck>`，框架在 `CheckHealthAsync` 被调用时从 DI 容器解析实例。

**理由**：
- 检查可以依赖任意 DI 服务（日志、配置、HttpClientFactory）
- 生命周期由框架管理
- 便于单元测试

### 3. DatabaseHealthCheck 统一接口，内部按类型分发

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly StorageConfiguration _storage;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        return _storage.Type switch
        {
            StorageTypes.POSTGRE   => await CheckPostgreSql(),
            StorageTypes.SQLSERVER => await CheckSqlServer(),
            StorageTypes.SQLITE    => await CheckSqlite(),
            StorageTypes.INMEMORY  => Healthy(),
            _ => Unhealthy($"Unknown storage type: {_storage.Type}")
        };
    }
}
```

**理由**：
- 一个检查类覆盖所有数据库类型，注册逻辑统一
- 每种数据库的检查方法独立，便于维护和测试
- 消除当前 `AddPostgreSqlReadinessCheck` 与 SQL Server 之间的命名/实现错配

**替代方案考虑**：每个数据库类型一个检查类 + 工厂模式。否决理由——过度设计，当前只有 4 种类型，单类内 switch 足够清晰。

### 4. KeyValueStoreHealthCheck 按类型区分

```csharp
public class KeyValueStoreHealthCheck : IHealthCheck
{
    private readonly KeyValueConfiguration _kvConfig;

    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        return _kvConfig.Type switch
        {
            KeyValueTypes.REDIS    => await CheckRedis(),
            KeyValueTypes.INMEMORY => Healthy(),
            _ => Healthy() // 未知类型默认健康
        };
    }
}
```

Redis 检查使用 `StackExchange.Redis` 发送 PING 命令。InMemory 始终健康。

### 5. MessageBrokerHealthCheck

```csharp
public class MessageBrokerHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        return _brokerConfig.Transport switch
        {
            TransportTypes.SQLSERVER => await CheckSqlServer(),
            TransportTypes.INMEMORY  => Healthy(),
            _ => Healthy()
        };
    }
}
```

SQL Server 传输检查：打开到传输数据库的 SQL 连接并执行 `SELECT 1`。

### 6. IdServerEndpointCheck 重写

```csharp
public class IdServerEndpointCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IdServerEndpointCheck> _logger;

    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        var client = _httpClientFactory.CreateClient("health-check");
        var response = await client.GetAsync(discoveryUrl, cancellationToken);
        // 验证 HTTP 200 + 响应体是合法 JSON
    }
}
```

**关键变化**：
- 使用 `IHttpClientFactory` 替代 `new HttpClient`（修复资源泄漏）
- 增加响应体 JSON 合法性验证（防止 200 但返回乱码）
- 删除 `ignoreCertificateError` 参数（由 `IHttpClientFactory` 的命名客户端配置处理）

### 7. MapIdentityHealthChecks 统一端点映射

```csharp
public static WebApplication MapIdentityHealthChecks(this WebApplication app)
{
    app.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => false,
        AllowCachingResponses = false
    });
    app.MapHealthChecks("/readyz", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        AllowCachingResponses = false
    });
    app.MapHealthChecks("/startupz", new HealthCheckOptions
    {
        Predicate = _ => false,
        AllowCachingResponses = false
    });
    return app;
}
```

放在 `MinGo.Identity.Configuration` 项目中。两个 Program.cs 各调用一次。

### 8. 结构化 JSON 响应

配置自定义 `ResponseWriter`：

```csharp
app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriters.WriteJson
});
```

输出格式：

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.045",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "PostgreSQL is reachable",
      "duration": "00:00:00.020"
    },
    "keyvault": {
      "status": "Healthy",
      "description": "Redis is reachable",
      "duration": "00:00:00.015"
    }
  }
}
```

使用 ASP.NET Core 内置的 `HealthReport` 对象直接序列化。

### 9. 注册流程总图

```
IdServer Startup:
  services.AddIdentityHealthChecks(configuration)
    ├── Read StorageConfiguration
    │   ├── POSTGRE   → register DatabaseHealthCheck (tag: ready)
    │   ├── SQLSERVER → register DatabaseHealthCheck (tag: ready)
    │   ├── SQLITE    → register DatabaseHealthCheck (tag: ready)
    │   └── INMEMORY  → skip
    ├── Read KeyValueConfiguration
    │   ├── REDIS     → register KeyValueStoreHealthCheck (tag: ready)
    │   └── INMEMORY  → skip
    └── Read MessageBrokerOptions
        ├── SQLSERVER → register MessageBrokerHealthCheck (tag: ready)
        └── INMEMORY  → skip

Admin Startup:
  services.AddIdentityHealthChecks(configuration)
    ├── Read IdentityServerAdminConfiguration
    │   ├── IdserverBaseUrl
    │   │   → register IdServerEndpointCheck (tag: ready)
    │   └── ScimBaseUrl (if non-empty)
    │       → register ScimEndpointCheck (tag: ready)
    └── (Admin 无 Storage/KV/MessageBroker 配置)
```

### 10. 文件组织

```
src/MinGo.Identity.Configuration/
├── HealthCheckExtensions.cs      ← 入口扩展方法 (Add + Map)
├── HealthChecks/
│   ├── DatabaseHealthCheck.cs    ← 数据库检查（全部存储类型）
│   ├── KeyValueStoreHealthCheck.cs ← Redis/InMemory 检查
│   ├── MessageBrokerHealthCheck.cs ← MassTransit 传输检查
│   └── IdServerEndpointCheck.cs ← OIDC 端点检查
└── HealthChecks/
    └── HealthCheckResponseWriters.cs ← JSON 响应 writer
```

## Risks / Trade-offs

- **[配置绑定时机]** `IConfiguration` 在 `AddIdentityHealthChecks` 调用时读取，如果后续配置变更不会反映到已注册的检查 → **缓解**：健康检查注册在启动时完成，运行时不重新读取配置，行为与现有模式一致
- **[Redis 不可用判定]** PING 成功不代表 Redis 能正常读写 → **缓解**：与社区标准做法一致，更深的检查属于应用级监控范畴
- **[SQLite 检查副作用]** 打开 SQLite 连接可能触发文件锁 → **缓解**：检查使用短连接，Open 后立即 Close，耗时控制在 100ms 内
- **[SCIM 端点检查误报]** Admin 配置了 ScimBaseUrl 但 SCIM 服务未启动不影响 Admin 核心功能 → **缓解**：SCIM 检查失败时降级为 Degraded 而非 Unhealthy，不影响整体 readiness 状态
- **[IHttpClientFactory 配置泄露]** 命名客户端配置可能泄漏到健康检查请求中 → **缓解**：使用独立的命名客户端配置（如 `"health-check"`），与业务客户端隔离
