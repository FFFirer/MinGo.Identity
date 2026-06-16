## Context

MinGo.Identity 包含两个 ASP.NET Core 服务：IdServer（OIDC/OAuth2 提供者，端口 5001）和 Admin（Blazor 管理面板，端口 5002）。当前两者均未实现应用级健康检查，仅依赖 Docker HEALTHCHECK 在容器层面使用业务端点进行检测。

生产环境中（docker-compose.prod.yml），PostgreSQL 已有独立的 healthcheck，但 IdServer 和 Admin 的 healthcheck 分别依赖 `/.well-known/openid-configuration` 和 `/master/clients` 业务端点，无法区分存活（进程活着）和就绪（依赖可用）两种状态。

## Goals / Non-Goals

**Goals:**
- 为 IdServer 和 Admin 项目添加 ASP.NET Core 内置健康检查端点
- 遵循 K8s 规范：`/healthz`（存活）、`/readyz`（就绪）、`/startupz`（启动）
- IdServer 就绪检查验证 PostgreSQL 数据库可达性
- Admin 就绪检查验证 IdServer OIDC 端点连通性
- 复用 `MinGo.Identity.Configuration` 共享库提供统一的扩展方法
- 更新 docker-compose.prod.yml 健康检查指向新端点
- 不在 Dockerfile 中定义 HEALTHCHECK（由 compose 层统一管理）
- 从 Dockerfile 中移除现有 HEALTHCHECK（由 compose 统一负责）

**Non-Goals:**
- 不添加健康检查 UI 仪表盘（如 AspNetCore.HealthChecks.UI）
- 不添加 Prometheus 指标导出（已有 OpenTelemetry）
- 不为健康检查端点添加鉴权（内部端点，匿名访问）
- 不修改 SimpleIdServer 框架代码
- 不处理 IdServer Hangfire/MassTransit 等内部组件的健康状态（仅检查关键外部依赖）

## Decisions

### 1. 端点路径风格：K8s 规范

| 端点 | 类型 | 说明 |
|------|------|------|
| `/healthz` | Liveness | 进程存活检查，始终返回 200 OK |
| `/readyz` | Readiness | 依赖就绪检查，依赖不可用返回 503 |
| `/startupz` | Startup | 启动完成检查，启动后返回 200 |

采用 K8s 规范路径，与容器编排平台原生对齐。

### 2. 实现模式：Configuration 共享库扩展方法

在 `MinGo.Identity.Configuration` 中提供：
- `AddIdentityHealthChecks()` — 注册健康检查服务
- `MapIdentityHealthChecks(this WebApplication app)` — 映射端点

选择理由：两个项目均需健康检查，提取到共享库避免重复代码。与项目中已有的 `ConfigurationHelper` 模式一致。

### 3. 就绪检查范围

- **IdServer**: PostgreSQL 数据库（通过 EF Core DbContext 或 Npgsql 直接连接）。这是最关键的外部依赖，数据库不可用则服务无法正常工作。
- **Admin**: IdServer OIDC Discovery 端点（通过 HTTP 请求 `{IdserverBaseUrl}/.well-known/openid-configuration`）。Admin 依赖 IdServer 进行认证，IdServer 不可达时 Admin 无法正常工作。
- **不包含**: Hangfire/MassTransit/缓存等内部组件 — 这些使用 INMEMORY 传输时无需检查，使用外部存储时可后续扩展。

### 4. NuGet 包选择

使用社区 `AspNetCore.HealthChecks.Npgsql` 而非 EF Core DbContext 检查，因为：
- IdServer 使用 SimpleIdServer 的内部 DbContext，非直接可注入
- Npgsql 健康检查直接验证 TCP+PG 协议层面的可达性
- EF Core Health Checks 需要 `IHealthCheck` 注册 DbContext 类型

Admin 使用自定义 `IHealthCheck` 实现 HTTP 端点检查。

### 5. Docker HEALTHCHECK 策略

Dockerfile 中不定义 HEALTHCHECK。所有 healthcheck 统一在 `docker-compose.prod.yml` 中定义，指向新的应用级健康检查端点。这样可以：
- 避免 Dockerfile 与 compose 之间的配置重复
- compose 层可以控制检查间隔、超时、重试次数
- 开发环境（docker-compose.yml）不强制 healthcheck，保持简单

### 6. 端点注册位置

健康检查端点应在中间件管道的**最前面**注册（在认证、授权、CORS 等中间件之前），以确保：
- 即使其他中间件出现故障，健康检查仍可响应
- 健康检查端点不依赖任何业务中间件
- 请求不会被 CORS 或认证逻辑拦截

## Risks / Trade-offs

- **[Dependency drift]** Admin 的就绪检查依赖 IdServer 的 `.well-known/openid-configuration` 端点。如果 IdServer 修改了此端点的行为，Admin 健康检查可能失效 → **缓解**: 使用简单的 HTTP 状态码检查，不解析响应体
- **[False negatives]** 如果网络短暂抖动，健康检查可能误报 → **缓解**: `HealthCheckOptions` 中配置 `Predicate` 和适当的超时时间
- **[False positives]** 数据库连接池中有可用连接但业务表损坏 → **缓解**: 基础健康检查通常不验证数据完整性，这是更深层的监控范畴
- **[Startup race condition]** Admin 在 IdServer 尚未完全启动时可能报告未就绪 → **缓解**: 这实际上是正确行为，等待就绪后再路由流量正是 readiness probe 的目的
