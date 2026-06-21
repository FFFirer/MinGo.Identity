## ADDED Requirements

### Requirement: SidServerSetup 调用 ConfigureDataseeder()

`SidServerSetup.ConfigureIdServer()` SHALL 在末尾调用 `ConfigureDataseeder(webApplicationBuilder, configuration)`。

`ConfigureDataseeder()` SHALL 注册以下 `IDataSeeder`：

| Seeder | Phase | 职责 |
|--------|-------|------|
| `DropDistributedCacheDataSeeder` | BeforeDeployment | 清理历史迁移遗留的分布式缓存表 |
| `ClientTypeDataSeeder` | BeforeDeployment | 转换旧版 ClientType 数据到新版枚举 |
| `ConfigureAdminWebsiteRedirectUrlsDataSeeder` | AfterDeployment | 配置 SIDS-manager 客户端的 redirect URL |
| `ConfigureSwaggerClientRedirectUrlsDataSeeder` | AfterDeployment | 配置 swaggerClient 的 redirect URL |
| `ConfigureAuthSchemeProviderDataSeeder` | AfterDeployment | 注册 Facebook/Google 认证方案 |
| `ConfigureCredentialIssuerDataSeeder` | AfterDeployment | 配置 Credential Issuer 客户端和范围 |
| `ConfigureFastfedDataSeeder` | AfterDeployment | 配置 FastFed 客户端和角色 |
| `ConfigureFederationDataseeder` | AfterDeployment | 为 SIDS-manager 添加 federation 范围 |
| `ConfigureGotifyDataseeder` | AfterDeployment | 配置 Gotify 通知会话 |

#### Scenario: ConfigureDataseeder 注册所有 seeder

- **WHEN** `SidServerSetup.ConfigureIdServer()` 执行
- **THEN** 上述 9 个 `IDataSeeder` 均已注册到 DI 容器
- **THEN** `SeedData()` 执行时这些 seeder 被依次调用

### Requirement: SeedAdministrationData baseUrl 从配置读取

`SeedAdministrationData` 的 redirect URL 参数 SHALL 使用 `configuration.AdminUiUrl`，而非当前 `string.Empty`。

`AdminUiUrl` 默认值为 `https://localhost:5002`，可通过 `appsettings.json` 或环境变量覆盖。

#### Scenario: SIDS-manager 获取正确 redirect URL

- **WHEN** `InitAdministrativeClientDataseeder` 执行
- **THEN** SIDS-manager 的 `RedirectionUrls` 包含 `{AdminUiUrl}/signout-callback-oidc`
- **THEN** SIDS-manager 的 `PostLogoutRedirectUris` 包含 `{AdminUiUrl}/signout-callback-oidc`

### Requirement: ConfigureAdminWebsiteRedirectUrlsDataSeeder 补全通配路径

`ConfigureAdminWebsiteRedirectUrlsDataSeeder` SHALL 从 `IdentityServerConfiguration.AdminUiUrl` 读取 Admin 地址。

该 seeder SHALL 为 SIDS-manager 添加：
- redirect URL: `{AdminUiUrl}/*`
- post-logout redirect URL: `{AdminUiUrl}/signout-callback-oidc`
- back-channel logout URL: `{AdminUiUrl}/bc-logout`

该 seeder SHALL 在 `InitAdministrativeClientDataseeder` 之后执行（由 `DataSeederExecutionHistoryRepository` 跟踪顺序）。

#### Scenario: SIDS-manager redirect URL 补全

- **WHEN** `ConfigureAdminWebsiteRedirectUrlsDataSeeder` 执行
- **THEN** SIDS-manager 的 `RedirectionUrls` 包含 `https://localhost:5002/*`
- **THEN** SIDS-manager 的 `PostLogoutRedirectUris` 包含 `https://localhost:5002/signout-callback-oidc`
- **THEN** SIDS-manager 的 `BackChannelLogoutUri` 为 `https://localhost:5002/bc-logout`
