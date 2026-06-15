## Why

Admin (MinGo.IdServer.Admin) 部署在 YARP 反向代理之后时，由于没有配置 `ForwardedHeadersOptions`，`HttpRequest.Host` 始终使用 YARP 到 Admin 的内部通信地址（容器 hostname），而非用户访问的公网域名。这导致 HTTPS 重定向、OIDC 登录回调等页面跳转地址错误，用户被跳转到无法访问的内部地址，认证流程中断。

## What Changes

- 在 Admin 的 `Program.cs` 中追加 `ForwardedHeadersOptions` 配置，认领 `XForwardedFor | XForwardedProto | XForwardedHost`
- 配置 `KnownNetworks.Clear()` 和 `KnownProxies.Clear()` 以信任容器内网的转发头
- 修改 `IdentityServerAdminConfiguration` 新增 `IsForwardedEnabled` 配置项，作为功能开关
- 配置仅当 `IsForwardedEnabled == true` 时生效，保持配置开关一致性
- 更新 `appsettings.json` 和 `appsettings.Docker.json` 添加 `IsForwardedEnabled: true`
- 更新 `docker-compose.prod.yml` 添加 `IsForwardedEnabled` 环境变量

## Capabilities

### New Capabilities
- `reverse-proxy-forwarded-headers`: 使 Admin（Blazor Server 管理后台）在 YARP 反向代理环境下正确还原客户端原始域名和协议，确保 HTTPS 重定向和 OIDC 认证回调使用正确的公网域名

### Modified Capabilities
无（不涉及现有能力的行为变更，仅修复基础设施层的 URL 构建）

## Impact

- **影响范围**：Admin 项目（`Program.cs`, `IdentityServerAdminConfiguration.cs`, `appsettings.json`, `appsettings.Docker.json`）以及 `docker/docker-compose.prod.yml`
- **外部依赖**：无新增 NuGet 包（仅依赖 ASP.NET Core 内置的 `ForwardedHeadersMiddleware`）
- **回滚安全**：设置 `IsForwardedEnabled: false` 即回退到原行为
- **方案一致性**：与现有 `fix-reverse-proxy-forwarded-host`（IdServer 侧）的 `ForwardedHeadersOptions` 配置方案保持一致
