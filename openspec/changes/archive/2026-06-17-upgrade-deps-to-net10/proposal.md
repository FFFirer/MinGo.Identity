## Why

项目已全面升级至 `net10.0` 目标框架，但核心依赖（EF Core、Microsoft.Extensions、Npgsql 等）仍停留在 9.x 版本。将相关依赖统一升级至 .NET 10 对应的最新稳定版本，确保运行时兼容性、获取最新安全修复和功能改进，并消除版本碎片化带来的维护风险。

## What Changes

### Microsoft 生态 → 10.0.9
- Microsoft.EntityFrameworkCore 及所有扩展包 (SqlServer/InMemory/Relational/Sqlite/Design/Tools)
- Microsoft.Extensions.Configuration 及扩展 (Binder/Json/EnvironmentVariables/UserSecrets/CommandLine)
- Microsoft.Extensions.Caching.SqlServer / StackExchangeRedis
- Microsoft.AspNetCore.Authentication.Facebook / Google / Negotiate
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
- Microsoft.VisualStudio.Web.CodeGeneration.Design
- Microsoft.NET.Test.Sdk 17.13.0 → 18.6.0

### 数据库 Provider
- Npgsql 9.0.3 → 10.0.3
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4 → 10.0.2

### 第三方独立包（升级至兼容 .NET 10 的最新稳定版）
- Serilog.AspNetCore 9.0.0 → 10.0.0
- OpenTelemetry.Exporter.Console / OpenTelemetryProtocol / Api: 1.15.x → 1.16.0
- Hangfire.PostgreSql 1.20.10 → 1.21.1
- Hangfire.SqlServer 1.8.18 → 1.8.23
- Community.Microsoft.Extensions.Caching.PostgreSql 4.0.6 → 6.0.2
- NeoSmart.Caching.Sqlite.AspNetCore 8.0.0 → 9.0.1
- Pomelo.Extensions.Caching.MySql 2.2.0 → 2.2.1
- Microsoft.Bcl.Memory 10.0.4 → 10.0.9
- System.Security.Cryptography.Xml 10.0.6 → 10.0.9

### 测试工具
- xunit.runner.visualstudio 2.8.2 → 3.1.5
- coverlet.collector 6.0.4 → 8.0.1

### 明确不升级
- SimpleIdServer 生态 (7.0.0) — 稳定不动
- SidFormBuilder (7.0.0) — 稳定不动
- Pomelo.EntityFrameworkCore.MySql (9.0.0) — EF Core 10 版未发布
- MassTransit (8.4.0) — 大版本升级需单独评估
- System.CommandLine (2.0.0-beta4) — 保持测试版暂不升级
- xunit (2.9.3) — 已是最新稳定版

## Capabilities

### New Capabilities
- 无新能力引入，纯基础设施依赖升级

### Modified Capabilities
- 无已有能力的需求变更，均为版本号更新

## Impact

- **影响范围**：仅 `Directory.Packages.props` 中的版本号变更
- **构建验证**：需执行 `dotnet restore` + `dotnet build` 确认编译通过
- **测试验证**：需执行 `dotnet test` 确认测试通过
- **无 API/行为变更**：纯版本号升级，不涉及代码逻辑修改
