## Why

SimpleIdServer 部署在 YARP 反向代理之后时，OIDC 协议流程中的重定向 URL 使用 YARP 到 IdServer 的内部通信地址（容器 IP 或内部 hostname）而非用户访问的公网域名，导致浏览器跳转到无法访问的内部地址，认证流程中断。

## What Changes

- 在 `Program.cs` 中补充 `ForwardedHeadersOptions` 配置，启用 `XForwardedHost` 转发头支持
- 配置 `KnownProxies`/`KnownNetworks` 以信任 YARP 容器转发的头部信息
- 改动范围仅限于 **1 个文件**（`src/MinGo.IdServer/Program.cs`），新增约 15 行配置代码
- 不改动 SimpleIdServer NuGet 包代码，不改动 YARP 反向代理配置，不改动 `SidServerSetup.cs`
- 不涉及数据库、API 契约、前端变更

## Capabilities

### New Capabilities
- `reverse-proxy-forwarded-headers`: 使 SimpleIdServer 在反向代理（YARP）环境下能正确识别客户端原始域名和协议，构建正确的重定向 URL

### Modified Capabilities
无（不涉及现有能力的行为变更，仅修复基础设施层的 URL 构建）

## Impact

- **影响范围**：仅 `src/MinGo.IdServer` 项目，仅 `Program.cs` 一个文件
- **外部依赖**：无新增 NuGet 包依赖
- **回滚安全**：去掉新增配置即回退到原行为
- **环境要求**：与 `appsettings.json` 中的 `IsForwardedEnabled: true` 配合使用
