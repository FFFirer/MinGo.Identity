## Why

当前 MinGo.Identity 的 IdServer 和 Admin 项目是基于 SimpleIdServer 6.0.4 手动搭建的，使用了简化的 Program.cs 配置和有限的包引用。SimpleIdServer 7.0.0 提供了官方 dotnet new 模板，生成了更完整的项目结构——包含 SidServerSetup 统一配置层、多存储后端支持、10+ 认证方式、OpenTelemetry、Hangfire、MassTransit 等企业级集成。通过从模板彻底重来，可以对齐官方最佳实践，降低维护成本，并利用模板内置的全面配置能力。

## What Changes

- **删除** `src/MinGo.IdServer/` 和 `src/MinGo.IdServer.Admin/` 两个现有项目
- **从模板脚手架**全新的 `MinGo.IdServer`（`dotnet new idserver`）和 `MinGo.IdServer.Admin`（`dotnet new idserveradmin`）
- **适配 CPM**：将模板生成的包引用迁移到 `Directory.Packages.props` 集中管理
- **配置 PostgreSQL**：将 `appsettings.json` 的 `StorageConfiguration.Type` 从 `INMEMORY` 改为 `POSTGRE`
- **精简包引用**：注释掉不需要的数据库（MySQL/SQLite/SQLServer）和其他非必要包，附说明
- **移植自定义配置**：保留 `AdminUserDataSeeder` 种子数据和端口/启动配置
- **更新解决方案**：使用 `.slnx` 格式引用新项目
- **保留基础架构**：`docker-compose.yml`、`Directory.Build.props`、`nuget.config` 不变

## Capabilities

### New Capabilities
- `idserver-core`: SimpleIdServer IdServer 核心服务，使用 SidServerSetup 统一配置，支持 PostgreSQL 存储和多认证方式
- `idserver-admin`: SimpleIdServer Admin 管理后台（Blazor Interactive Server），通过 OIDC 连接 IdServer

### Modified Capabilities
- 无（初始构建，无既有 spec）

## Impact

- **删除代码**: `src/MinGo.IdServer/` 和 `src/MinGo.IdServer.Admin/` 全部现有文件
- **新增文件**: 模板生成的完整项目文件，含 SidServerSetup.cs、Areas、Views、Resources、wwwroot、Components 等
- **依赖变动**: 从手动精选的 ~10 个 SIS 包变为模板的 ~30+ 包（通过注释精简）
- **配置变动**: appsettings.json 从简化版变为模板完整版（157 行），StorageConfiguration 支持多后端切换
- **构建系统**: 保持 CPM（Directory.Packages.props）+ .slnx + net10.0
