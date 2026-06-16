## Why

当前 IdServer 和 Admin 项目仅依赖 Docker HEALTHCHECK 使用业务端点（`/.well-known/openid-configuration`、`/master/clients`）进行健康检测。这种方式将健康状态与业务逻辑耦合，无法区分存活（process alive）和就绪（dependencies ready）两种状态。在 Kubernetes 等容器编排环境中，需要独立的 `/healthz`（存活探针）和 `/readyz`（就绪探针）端点来实现正确的流量管理和滚动更新。

## What Changes

- 在 `MinGo.Identity.Configuration` 项目中新增 `HealthCheckExtensions.cs`，提供统一的健康检查注册和端点映射扩展方法
- IdServer 项目：注册健康检查服务（PostgreSQL 依赖检查），映射 `/healthz`、`/readyz`、`/startupz` 端点
- Admin 项目：注册健康检查服务（IdServer OIDC 端点连通性检查），映射 `/healthz`、`/readyz`、`/startupz` 端点
- 更新 `docker-compose.prod.yml` 中的 HEALTHCHECK 指令，指向新的健康检查端点
- 从 Dockerfile 中移除现有的 HEALTHCHECK 指令（由 compose 层统一管理）
- 新增 NuGet 包：`Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`、`Npgsql` 扩展

## Capabilities

### New Capabilities
- `health-checks`: 为 IdServer 和 Admin 项目提供标准化的应用级健康检查端点，支持存活/就绪/启动三种探针

### Modified Capabilities

无现有 spec 被修改。

## Impact

- **新增文件**: `src/MinGo.Identity.Configuration/HealthCheckExtensions.cs`
- **修改文件**:
  - `src/MinGo.IdServer/Conf/SidServerSetup.cs` — 注册健康检查
  - `src/MinGo.IdServer/Program.cs` — 映射端点
  - `src/MinGo.IdServer.Admin/Program.cs` — 注册 + 映射
  - `docker/Dockerfile.idserver` — 移除 HEALTHCHECK（由 compose 统一管理）
  - `docker/Dockerfile.admin` — 移除 HEALTHCHECK（由 compose 统一管理）
  - `docker/docker-compose.prod.yml` — 更新 healthcheck 端点路径和 `depends_on` 条件
  - `Directory.Packages.props` — 新增包版本定义
- **新增依赖**: `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`、`AspNetCore.HealthChecks.Npgsql`
