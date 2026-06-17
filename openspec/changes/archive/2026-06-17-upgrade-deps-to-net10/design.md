## Context

MinGo.Identity 已全面采用 `net10.0` 作为目标框架（定义在 `Directory.Build.props` 中），并使用 Central Package Management（`Directory.Packages.props`）集中管理所有 NuGet 包版本。当前大量 Microsoft 生态依赖仍锁定在 9.0.x 版本，与实际运行时 .NET 10 不匹配。本次设计聚焦于纯版本升级，不涉及架构或 API 变更。

核心约束：
- **Single source of truth**：所有版本号集中在 `Directory.Packages.props`，各 csproj 通过 CPM 引用
- **Zero behavioral change**：升级后功能行为完全一致，仅版本号变更
- **验证门禁**：`dotnet restore` → `dotnet build` → `dotnet test` 全链路通过

## Goals / Non-Goals

**Goals:**
- 将所有 Microsoft 生态包 (EF Core, Extensions, ASP.NET Core Auth 等) 从 9.0.4 升级至 10.0.9
- 将 Npgsql 及 Npgsql.EntityFrameworkCore.PostgreSQL 升级至支持 EF Core 10 的版本
- 将第三方独立包升级至兼容 .NET 10 的最新稳定版
- 将测试工具链升级至最新稳定版
- 构建与测试全量通过验证

**Non-Goals:**
- 不升级 SimpleIdServer 及 SidFormBuilder 版本
- 不升级 Pomelo.EntityFrameworkCore.MySql（EF Core 10 版未发布）
- 不升级 MassTransit（大版本升级需单独评估）
- 不升级 System.CommandLine（保持当前 beta 版本）
- 不修改任何 C# 代码或项目结构
- 不引入新的 NuGet 包

## Decisions

| 决策 | 方案 | 理由 |
|------|------|------|
| Microsoft 生态版本 | 10.0.9 | 最新稳定版，已验证兼容 net10.0 |
| Npgsql.EFCore.PostgreSQL | 10.0.2 | 针对 EF Core 10 的最新非预览版 |
| Npgsql | 10.0.3 | 与 EFCore 扩展版本匹配 |
| Serilog.AspNetCore | 10.0.0 | 支持 .NET 10 的最新稳定版 |
| OpenTelemetry | 1.16.0 | 最新稳定版，兼容 .NET 10 |
| Hangfire.PostgreSql | 1.21.1 | 最新稳定版 |
| Hangfire.SqlServer | 1.8.23 | 保持与 Hangfire.Core 同一大版本 |
| Microsoft.NET.Test.Sdk | 18.6.0 | 最新稳定版，支持 .NET 10 |
| xunit.runner.visualstudio | 3.1.5 | 最新稳定版 |
| coverlet.collector | 8.0.1 | 最新稳定版 |

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| Npgsql.EFCore.PostgreSQL 10.0.2 可能存在未发现的兼容问题 | 保持 CentralPackageTransitivePinningEnabled=true，锁定传递依赖版本 |
| OpenTelemetry 1.15.x → 1.16.0 可能引入 instrumentation 行为变更 | 升级后运行集成测试验证 tracing/metrics 正常工作 |
| coverlet.collector 6.0.4 → 8.0.1 跨大版本，配置格式可能变化 | 测试项目使用默认配置，无自定义 coverlet 设置，风险低 |
| xunit.runner.visualstudio 2.x → 3.x 可能有 runner 行为变更 | 检查 xunit 测试是否正常发现和执行 |
