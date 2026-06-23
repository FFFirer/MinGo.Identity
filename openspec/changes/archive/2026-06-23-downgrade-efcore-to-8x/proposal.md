## Why

SimpleIdServer.IdServer.Store.EF 7.0.0 声明 `Microsoft.EntityFrameworkCore >= 8.0.10`，但项目通过 CPM 强制升级到 EFCore 10.0.9，造成版本偏离。这导致 NU1608 警告（Pomelo.EntityFrameworkCore.MySql 传递依赖 8.0.2 与 EFCore 10.x 不兼容），且运行时可能产生未预期的行为。

降级到 EFCore 8.x 以对齐 SimpleIdServer 官方依赖声明，消除版本偏离风险。

## What Changes

- **Directory.Packages.props**：将所有 EFCore 相关包从 10.0.9 降级到 8.x (8.0.28)：
  - Microsoft.EntityFrameworkCore 及其衍生包（Design, Tools, SqlServer, InMemory, Relational, Sqlite）
  - Npgsql.EntityFrameworkCore.PostgreSQL 从 10.0.2 → 8.0.10（匹配 SIS PostgreMigrations 声明）
  - Pomelo.EntityFrameworkCore.MySql 从 9.0.0 → 8.0.2（匹配 SidFormBuilder 传递依赖）
- **MinGo.Identity.Migrator.csproj**：移除 NU1608 的 `<NoWarn>`（降级后兼容）
- **无代码改动**：项目仅使用基础 EFCore API，无需迁移

**保持不变**：所有非 EFCore 包（Microsoft.Extensions.* 10.0.9、Caching 包、OpenTelemetry、ASP.NET Core、Serilog 等）。

## Capabilities

### New Capabilities
<!-- 本项目为纯依赖降级，无新能力引入 -->

### Modified Capabilities
<!-- 无 spec 级需求变更，仅实现层面的依赖版本调整 -->

## Impact

- **依赖**：EFCore 8.x + .NET 10 Runtime（.NET 向后兼容，完全可行）
- **项目文件**：仅修改 Directory.Packages.props 和 Migrator.csproj
- **构建**：NU1608 消除，无新警告
- **运行时**：行为无变化，SimpleIdServer 依赖链对齐
