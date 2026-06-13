## Purpose

Define how SimpleIdServer handles `X-Forwarded-*` headers when deployed behind a reverse proxy (YARP), ensuring OIDC redirect URLs use the correct public-facing domain, protocol, and client IP.

## Requirements

### Requirement: 反向代理转发头正确识别

SimpleIdServer 在反向代理（YARP）环境下，SHALL 从 `X-Forwarded-Host`、`X-Forwarded-Proto`、`X-Forwarded-For` 转发头中正确还原客户端原始请求的域名、协议和 IP 地址，用于构建 OIDC 协议中的重定向 URL。

#### Scenario: 反向代理环境下重定向 URL 使用域名

- **WHEN** 用户通过 `https://openid.example.com` 访问 IdServer 的 OIDC authorize 端点
- **AND** 请求经过 YARP 转发，携带 `X-Forwarded-Host: openid.example.com`、`X-Forwarded-Proto: https`
- **THEN** IdServer 返回的 302 重定向 `Location` 使用 `https://openid.example.com` 作为 base URL
- **AND** 不包含容器内部地址（如 `http://idserver:5001`）

#### Scenario: `X-Forwarded-Proto` 未启用 HTTPS 时保持原样

- **WHEN** 反向代理未转发 `X-Forwarded-Proto: https`
- **AND** `ForceHttps` 为 `false`
- **THEN** IdServer 使用请求的实际协议（`http`）构建 URL

#### Scenario: 非反向代理环境下行为不变

- **WHEN** 请求直接到达 IdServer，未经过反向代理
- **THEN** IdServer 使用请求本身的 Host 和 Scheme 构建 URL，行为与修复前一致

#### Scenario: KnownProxies 配置不当导致转发头被忽略

- **WHEN** `KnownNetworks` 未清空且 YARP IP 不在 loobpack 范围内
- **THEN** `ForwardedHeadersMiddleware` 忽略所有 `X-Forwarded-*` 头
- **AND** IdServer 降级为使用请求本身的 Host 和 Scheme
- **AND** 这是已知的配置错误场景，需在部署时验证 `KnownNetworks`/`KnownProxies` 配置

### Requirement: 配置与现有配置开关一致

转发头补充配置 SHALL 仅在 `IsForwardedEnabled == true` 时生效，与 SimpleIdServer 的 `ForwardHttpHeader()` 调用条件保持一致。

#### Scenario: `IsForwardedEnabled` 为 true 时生效

- **WHEN** `appsettings.json` 中 `IsForwardedEnabled` 为 `true`
- **THEN** `Program.cs` 中的补充配置被应用
- **AND** `ForwardedHeadersOptions.ForwardedHeaders` 包含 `XForwardedFor | XForwardedProto | XForwardedHost`

#### Scenario: `IsForwardedEnabled` 为 false 时跳过

- **WHEN** `appsettings.json` 中 `IsForwardedEnabled` 为 `false`
- **THEN** `Program.cs` 中的补充配置被跳过
- **AND** `ForwardedHeadersOptions` 保持 SimpleIdServer 默认值（或不注册）
