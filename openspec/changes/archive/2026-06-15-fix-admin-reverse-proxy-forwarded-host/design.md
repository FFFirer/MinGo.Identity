## Context

### 当前架构

```
用户浏览器 ── HTTPS ──→ YARP (反向代理) ── HTTP ──→ Admin (:5002)
                         │                      (Blazor Server)
                         │ X-Forwarded-Host:
                         │   admin.example.com   ❌ 无 ForwardedHeaders 配置
                         │ X-Forwarded-Proto:    HttpRequest.Host = 内部地址
                         │   https               UseHttpsRedirection / OIDC 回调 均错误
                         │ X-Forwarded-For: ...
```

### 根因

Admin `Program.cs` 中没有任何 `ForwardedHeadersOptions` 配置。ASP.NET Core 的 `ForwardedHeadersMiddleware` 未被正确启用，导致 `X-Forwarded-*` 请求头被静默忽略，`HttpRequest.Host` 始终为 YARP 转发时使用的容器内部地址。

### 影响的具体场景

| 场景 | 依赖 Host 的方式 | 后果 |
|------|-----------------|------|
| `UseHttpsRedirection()` | 用 `HttpRequest.Host` 构造 302 跳转 URL | 跳转到内部地址（如 `http://admin:5002/...`）|
| OIDC 登录 `redirect_uri` | OIDC 中间件用 `HttpRequest.Host` 构造回调 URL | IdServer 回调到内部地址，浏览器无法访问 |
| `ForceHttps()` | SimpleIdServer 内部逻辑 | 同样生成错误的重定向地址 |
| Blazor SignalR 协商 | 浏览器连接 Admin 的信号端点 | 可能协商到内部地址 |

### 与 IdServer 侧修复的关系

现有 change `fix-reverse-proxy-forwarded-host` 已修复 IdServer 侧的同等问题，本设计与之对齐：

| 维度 | IdServer 方案 | Admin 方案 |
|------|-------------|-----------|
| 配置方式 | `Configure<ForwardedHeadersOptions>` 叠加 | 同左 |
| ForwardedHeaders 掩码 | `For \| Proto \| Host` | 同左 |
| KnownNetworks | `Clear()` | 同左 |
| 开关条件 | `IsForwardedEnabled == true` | 同左（需新增配置项）|
| 中间件注册 | `UseSid()` 内部自动调用 | 需确认 `UseIdserverAdmin()` 是否内置调用 |

## Goals / Non-Goals

**Goals:**

- Admin 在 YARP 反向代理后，`HttpRequest.Host` 使用 `X-Forwarded-Host` 透传的公网域名
- `UseHttpsRedirection()`、OIDC 回调、`ForceHttps()` 等页面跳转使用正确的公网域名
- 改动最小化，与 IdServer 侧方案保持一致
- 保持 `IsForwardedEnabled` 配置开关的一致性

**Non-Goals:**

- 不修改 YARP 的配置（已在反向代理层正确设置）
- 不涉及 IdServer 侧的任何改动
- 不增加新的 NuGet 依赖
- 不修改 UI 逻辑或认证业务逻辑
- 不涉及 SimpleIdServer NuGet 包源码

## Decisions

### 决策 1：配置叠加方式

与 IdServer 侧相同，采用 `Configure<ForwardedHeadersOptions>` 叠加配置：

```csharp
// SimpleIdServer 内部可能已有默认注册（或无）
// 我们通过后注册覆盖，追加 Host 并清空 KnownNetworks
Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                             | ForwardedHeaders.XForwardedProto
                             | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
```

### 决策 2：KnownProxies 策略

采用与 IdServer 侧一致的**方案 B**：`KnownNetworks.Clear()` + `KnownProxies.Clear()`

| 策略 | 安全 | 可维护性 | 适用场景 |
|------|------|----------|----------|
| A. 指定 YARP IP | 高 | 低（IP 可能随部署变化） | 固定 IP 环境 |
| **B. 清空限制** | 中 | 高（无运维成本） | Docker 内网，不直接暴露 |

理由：
- Admin 容器仅通过 YARP 可达，不直接对外暴露
- 避免容器重建导致 IP 变化的运维负担

### 决策 3：配置开关

在 `IdentityServerAdminConfiguration` 中新增 `IsForwardedEnabled` 属性，与 IdServer 的 `IdentityServerConfiguration` 一致。

### 决策 4：中间件注册顺序

需要确认 `UseIdserverAdmin()` 内部是否调用了 `UseForwardedHeaders()`。如果未调用，需在 `app.UseIdserverAdmin()` **之前**手动添加 `app.UseForwardedHeaders()`。

参考 SimpleIdServer 的 `UseSid()` 实现——其在 `UseRouting()` 之前调用了 `UseForwardedHeaders()`，这是正确的顺序：转发头处理必须在路由之前。

验证方式：`UseIdserverAdmin()` → 是否内部调用 `UseForwardedHeaders()`。如果未调用，需在 `Program.cs` 中 `Build()` 之后、`UseIdserverAdmin()` 之前手动添加：

```csharp
if (idserverAdminConfiguration.IsForwardedEnabled)
{
    app.UseForwardedHeaders();
}
```

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| `KnownNetworks.Clear()` 信任所有来源的 `X-Forwarded-*` 头 | 内网中的其他容器可伪造客户端 IP/域名 | Admin 仅通过 YARP 可达，不直接暴露；敏感操作依赖认证令牌而非 IP |
| `Configure<T>` 叠加行为不直观 | 未来 SimpleIdServer 升级可能产生冲突 | 注释明确标注覆盖关系 |
| `UseIdserverAdmin()` 内部中间件顺序不确定 | 如果未先调用 `UseForwardedHeaders()`，配置不生效 | 验证后决定是否需要手动添加 `app.UseForwardedHeaders()` |
