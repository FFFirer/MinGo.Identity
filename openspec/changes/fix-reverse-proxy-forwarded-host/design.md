## Context

### 当前架构

```
用户浏览器 ── HTTPS ──→ YARP (反向代理) ── HTTP ──→ SimpleIdServer (:5001)
                         │                         │
                         │ X-Forwarded-Host:        │ ForwardHttpHeader() 启用:
                         │   openid.example.com     │   For | Proto (缺少 Host)
                         │ X-Forwarded-Proto: https │
                         │ X-Forwarded-For: ...     │
```

### 根因链

三个条件需**同时满足**才能正确还原客户端域名：

| # | 条件 | 当前状态 |
|---|------|----------|
| 1 | YARP 发送 `X-Forwarded-Host` 头 | ✅ 已配置 |
| 2 | `ForwardedHeadersOptions` 认领 `XForwardedHost` | ❌ **缺失** |
| 3 | `KnownProxies` 允许信任 YARP 的 IP | ⚠️ 需确认 |

#### 条件 2 详细分析

SimpleIdServer 的 `ForwardHttpHeader()` 内部实现（`IdServerBuilder.cs`）：

```csharp
public IdServerBuilder ForwardHttpHeader()
{
    Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                 | ForwardedHeaders.XForwardedProto;
        // 缺少 ForwardedHeaders.XForwardedHost
    });
}
```

`UseSid()` 内部自动调用 `webApplication.UseForwardedHeaders()`，中间件根据 DI 中注册的 `ForwardedHeadersOptions` 决定处理哪些头部。由于 `Host` 不在 `ForwardedHeaders` 掩码中，`X-Forwarded-Host` 被忽略，`HttpRequest.Host` 保持为 YARP 转发时使用的内部地址。

#### 条件 3 详细分析

ASP.NET Core `ForwardedHeadersMiddleware` 默认只信任来自 `127.0.0.0/8` 和 `::1` 的转发头。在 Docker 容器部署场景中，YARP 和 IdServer 处于不同容器（通过 `mingo-network` 通信），YARP 的 IP 不在 loopback 范围内——因此即使条件 2 修复了，转发头仍然会被静默丢弃。

### 现有配置

- `appsettings.json` 中 `IsForwardedEnabled: true`
- `SidServerSetup.cs` 第 138-140 行：`if (configuration.IsForwardedEnabled) idServerBuilder.ForwardHttpHeader();`
- YARP 已配置 `X-Forwarded-Host: $host`

## Goals / Non-Goals

**Goals:**

- SimpleIdServer 在 YARP 反向代理后的 OIDC 重定向 URL 使用公网域名而非内部地址
- 改动最小化：仅修改 `Program.cs`，不触及 SimpleIdServer 的 NuGet 包或模板代码
- 保持与 `IsForwardedEnabled` 配置开关的一致性
- 支持 Docker 容器化部署场景

**Non-Goals:**

- 不修改 YARP 的配置（已正确设置）
- 不修改 `SidServerSetup.cs`（模板代码，通过叠加配置覆盖即可）
- 不修改认证业务逻辑
- 不增加新的 NuGet 依赖
- 不涉及前端或 Admin 项目

## Decisions

### 决策 1：配置叠加而非修改

| 选项 | 方案 |
|------|------|
| 方式 | 在 `Program.cs` 中 `SidServerSetup.ConfigureIdServer()` 之后，追加 `Configure<ForwardedHeadersOptions>` |
| 原理 | ASP.NET Core Options 系统按注册顺序执行 `IConfigureOptions<T>`，后注册的覆盖同名属性 |
| 无需修改 | `SidServerSetup.cs`、SimpleIdServer 源码、任何 NuGet 包 |

配置顺序：
```csharp
// 第 1 步：SimpleIdServer 内部注册（ForwardHttpHeader）
Configure<ForwardedHeadersOptions>(o => o.ForwardedHeaders = For | Proto)

// 第 2 步：我们的补充（覆盖）
Configure<ForwardedHeadersOptions>(o => {
    o.ForwardedHeaders = For | Proto | Host;  // 补上 Host
    o.KnownNetworks.Clear();                   // 信任容器内网
})
```

### 决策 2：`KnownProxies` 策略

| 策略 | 安全 | 可维护性 | 推荐场景 |
|------|------|----------|----------|
| A. 指定 YARP IP (`KnownProxies.Add`) | 高 | 低（IP 可能随部署变化） | 固定 IP 环境 |
| B. 清空限制 (`KnownNetworks.Clear`) | 中 | 高（无运维成本） | Docker 内网，IdServer 不暴露公网 |

**推荐方案 B**：`KnownNetworks.Clear()` + `KnownProxies.Clear()`

理由：
- IdServer 容器仅在内网暴露（`mingo-network`），不直接对外提供服务
- 清空后信任来自**所有来源**的转发头——在此架构中是安全的
- 避免容器重建导致 IP 变化时的运维负担
- 如果将来有更高安全要求，可升级到方案 A 或使用 `ForwardLimit = 1`

### 决策 3：配置保护条件

仅在 `IsForwardedEnabled == true` 时生效，与现有 `ForwardHttpHeader()` 的调用条件一致：
```csharp
if (identityServerConfiguration.IsForwardedEnabled)
{
    // 补充配置
}
```

### 决策 4：不修改 `UseSid()` 的内部顺序

`UseSid()` 内部会在 `UseRouting()` 之前调用 `UseForwardedHeaders()`，这是正确的中间件顺序——转发头处理必须在路由之前，以确保路由能使用正确的 Host。我们无需干预此顺序。

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| `KnownNetworks.Clear()` 信任所有来源的 `X-Forwarded-*` 头 | 内网中的其他容器可伪造客户端 IP/域名 | IdServer 仅通过 YARP 可达，不直接暴露在共享网络中；敏感操作依赖认证令牌而非 IP |
| `Configure<T>` 多次调用的叠加行为不直观 | 如果未来 SimpleIdServer 升级改变了配置逻辑，可能产生意外冲突 | 在注释中明确标注覆盖关系；升级时需验证此配置 |
| `X-Forwarded-Host` 被信任后，如果 YARP 配置错误（发送错误 host）会导致 URL 错误 | 重定向到错误的域名 | YARP 已正确配置；此类问题在测试阶段即可发现 |
| `app.UseForwardedHeaders()` 在 `UseSid()` 内部自动调用，我们无法控制其中间件位置 | 如果中间件注册顺序有问题可能不生效 | 已验证 `UseSid()` 内部调用的时序正确 |

## Open Questions

无——所有技术决策已在上面的分析中闭环。
