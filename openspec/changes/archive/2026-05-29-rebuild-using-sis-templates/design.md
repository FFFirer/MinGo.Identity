## Context

当前 MinGo.Identity 包含两个 ASP.NET Core 项目：
- `MinGo.IdServer`（:5001）- 基于 SimpleIdServer 6.0.4 手写配置
- `MinGo.IdServer.Admin`（:5002）- Admin 管理后台

SimpleIdServer 7.0.0 官方模板提供了更完整的项目骨架。本次设计旨在通过模板彻底重建两个项目，同时保持：
- PostgreSQL 存储（模板默认 SQL Server）
- CPM 包管理（Directory.Packages.props）
- 自定义种子数据（AdminUserDataSeeder）
- net10.0 目标框架

## Goals / Non-Goals

**Goals:**
- 从 `dotnet new idserver` 和 `dotnet new idserveradmin` 模板脚手架全新项目
- 迁移到 CPM 集中管理包版本
- 配置 PostgreSQL 存储（通过 appsettings.json 的 StorageConfiguration.Type）
- 注释掉不需要的包引用（附中文说明），保留核心功能
- 移植自定义种子数据 AdminUserDataSeeder
- 保持端口 5001（IdServer）和 5002（Admin）
- 使用 `.slnx` 解决方案格式

**Non-Goals:**
- 不额外添加 SCIM 服务（将来如需可单独搭建）
- 不添加 SAML / WsFed / FIDO2 等当前不需要的协议支持
- 不配置 OpenTelemetry（注释保留，将来按需启用）
- 不修改 docker-compose.yml（PostgreSQL 保持不变）

## Decisions

| # | 决策 | 选择 | 备选方案 | 理由 |
|---|------|------|---------|------|
| D1 | 项目重建方式 | `dotnet new` 脚手架全新生成 | 手工改造现有项目 | 模板包含 SidServerSetup 统一配置层和多存储后端切换，手动对齐成本高 |
| D2 | 存储配置 | appsettings.json 的 `StorageConfiguration.Type` 设为 `POSTGRE` | 硬编码 UseNpgsql | 模板内置多后端支持，改配置即可，未来可切换到其他数据库 |
| D3 | 包管理 | CPM（Directory.Packages.props） | 模板自带的内联版本 | 项目已用 CPM，保持一致 |
| D4 | 多余包处理 | 注释掉 + 中文说明 | 直接删除 | 保留可追溯性，将来按需取消注释即可启用对应功能 |
| D5 | 种子数据 | 移植 AdminUserDataSeeder 到模板的 Conf/Migrations 体系 | 保持独立 seeder | 模板已有 `IDataSeeder` 接口和 AfterDeployment 体系，整合更规范 |
| D6 | 启动配置 | 保留 launchSettings.json（5001/5002） | 默认模板设置 | 模板已默认 5001/5002，仅调整 applicationUrl 格式 |

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| 模板生成大量文件（Areas/Views/Resources），增加项目体积 | 这是标准 SIS 模板输出，所有 UI 文件为运行时必需，不删除 |
| 包引用数量从 ~10 暴增到 ~30+ | 用注释标记不需要的包，构建时不影响（不引用则不会被加载） |
| SidServerSetup.cs 功能过于丰富（含不需要的 MassTransit/Hangfire/OpenTelemetry） | 这些功能通过配置控制，不改代码只改配置即可禁用 |
| net10.0 可能有不兼容的 API 变化 | 模板直接生成 net10.0，已验证兼容 |
