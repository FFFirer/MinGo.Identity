## ADDED Requirements

### Requirement: Admin 反向代理转发头正确识别

Admin（Blazor Server 管理后台）在反向代理（YARP）环境下，SHALL 从 `X-Forwarded-Host`、`X-Forwarded-Proto`、`X-Forwarded-For` 转发头中正确还原客户端原始请求的域名、协议和 IP 地址，用于 HTTPS 重定向和 OIDC 认证回调。

#### Scenario: HTTPS 重定向使用公网域名

- **WHEN** 用户通过 `http://admin.example.com` 访问 Admin
- **AND** YARP 将请求以 HTTP 转发到 Admin 内部地址 `http://admin:5002`
- **AND** YARP 携带 `X-Forwarded-Host: admin.example.com`、`X-Forwarded-Proto: https`
- **THEN** `UseHttpsRedirection()` 将用户重定向到 `https://admin.example.com`（而非 `https://admin:5002`）

#### Scenario: OIDC 登录回调使用公网域名

- **WHEN** 用户通过 `https://admin.example.com` 访问 Admin 触发 OIDC 登录
- **AND** 请求经过 YARP 转发，携带 `X-Forwarded-Host: admin.example.com`、`X-Forwarded-Proto: https`
- **THEN** OIDC 中间件构造的 `redirect_uri` 使用 `https://admin.example.com` 作为 base URL
- **AND** IdServer 能够正确回调到公网可达的 Admin 地址

#### Scenario: 非反向代理环境下行为不变

- **WHEN** 请求直接到达 Admin，未经过反向代理
- **THEN** Admin 使用请求本身的 Host 和 Scheme 构建重定向 URL，行为与修复前一致

#### Scenario: KnownProxies 配置不当导致转发头被忽略

- **WHEN** `KnownNetworks` 未清空且 YARP IP 不在 loopback 范围内
- **THEN** `ForwardedHeadersMiddleware` 忽略所有 `X-Forwarded-*` 头
- **AND** Admin 降级为使用请求本身的 Host 和 Scheme

### Requirement: 配置与配置开关一致

转发头补充配置 SHALL 仅在 `IsForwardedEnabled == true` 时生效。

#### Scenario: `IsForwardedEnabled` 为 true 时生效

- **WHEN** `appsettings.json` 中 `IsForwardedEnabled` 为 `true`
- **THEN** `Program.cs` 中的补充配置被应用
- **AND** `ForwardedHeadersOptions.ForwardedHeaders` 包含 `XForwardedFor | XForwardedProto | XForwardedHost`

#### Scenario: `IsForwardedEnabled` 为 false 时跳过

- **WHEN** `appsettings.json` 中 `IsForwardedEnabled` 为 `false`
- **THEN** `Program.cs` 中的补充配置被跳过
- **AND** `ForwardedHeadersOptions` 保持未配置状态

### Requirement: 中间件注册顺序正确

`UseForwardedHeaders()` SHALL 在 `UseRouting()` 之前注册，以确保路由使用正确的 Host。

#### Scenario: 中间件在路由之前注册

- **WHEN** Admin 应用启动
- **AND** `IsForwardedEnabled` 为 `true`
- **THEN** `UseForwardedHeaders()` 在 `UseRouting()` 之前被调用
- **AND** 路由中间件使用正确的 Host 进行请求匹配

### Requirement: `ForwardedHeadersOptions` 配置与 IdServer 侧一致

Admin 的 `ForwardedHeadersOptions` SHALL 认领 `XForwardedFor | XForwardedProto | XForwardedHost`，与 IdServer 侧的补充配置保持一致。

#### Scenario: Host 转发头被认领

- **WHEN** `IsForwardedEnabled` 为 `true`
- **THEN** `ForwardedHeaders` 包含 `XForwardedHost`
- **AND** `X-Forwarded-Host` 请求头的值被用于构造 `HttpRequest.Host`
