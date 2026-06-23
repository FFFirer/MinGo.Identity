## Context

当前项目通过 CPM (Directory.Packages.props) 将 EFCore 系列包锁定在 10.0.9，但 SimpleIdServer 7.0.0 的 Store.EF 包声明 `Microsoft.EntityFrameworkCore >= 8.0.10`。这导致两个问题：

1. **版本偏离**：运行时可能使用未被 SimpleIdServer 充分测试的 EFCore 10.x，引入未预期行为
2. **NU1608 警告**：Pomelo.EntityFrameworkCore.MySql 8.0.2（来自 SidFormBuilder 的传递依赖）与 EFCore 10.x 不兼容，产生 NU1608 警告

项目目标框架为 net10.0，运行时为 .NET 10，.NET 对低版本库完全向后兼容。

## Goals / Non-Goals

**Goals:**
- 将所有 EFCore 包降级到 8.x，对齐 SimpleIdServer 7.0.0 依赖声明
- 消除 NU1608 构建警告
- 保持目标框架 net10.0 不变
- 保持所有非 EFCore 包版本不变

**Non-Goals:**
- 不改变项目代码逻辑
- 不改变运行时版本 (.NET 10)
- 不改变其他 NuGet 包版本

## Decisions

### 1. 目标版本选择：8.0.28（而非 8.0.10）

EFCore 8.x 最新稳定版是 8.0.28（2026-06-09 发布）。选择最新补丁版本以获取安全修复和 bugfix。SimpleIdServer 只声明 `>= 8.0.10`，8.0.28 完全满足要求。

### 2. Npgsql.EntityFrameworkCore.PostgreSQL：8.0.10

这是 Npgsql 为 EFCore 8.x 发布的最后一个版本。SimpleIdServer.IdServer.PostgreMigrations 7.0.0 声明 `Npgsql.EntityFrameworkCore.PostgreSQL >= 8.0.10`。

### 3. Pomelo.EntityFrameworkCore.MySql：8.0.2

这是 Pomelo 为 EFCore 8.x 发布的最后一个版本。对齐 SidFormBuilder 的传递依赖版本，彻底消除 NU1608。

### 4. Npgsql ADO.NET 驱动：保持不受 CPM 强制覆盖

Npgsql 10.0.3 作为 Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2 的传递依赖被带高，降级后 Npgsql.EFCore 8.0.10 会拉取 Npgsql 8.x 系列。CPM 未显式锁定 Npgsql 版本，由传递依赖决定。

### 5. OpenTelemetry.Instrumentation.EntityFrameworkCore：保持 1.15.1-beta.1

该包完全不依赖 EFCore 编译。它通过 DiagnosticSource 事件监听工作，与 EFCore 版本解耦，无需降级。

## Risks / Trade-offs

- **[风险] EFCore 8.x 不支持 .NET 10 的新 CSP 分析器** → 无影响，项目未启用相关分析器
- **[风险] 未来 EFCore 8.x 安全补丁** → 8.x 仍在支持周期内 (LTS)，将持续接收补丁
- **[非风险] 性能差异** → EFCore 8.x 和 10.x 在查询性能上无显著差异
- **[非风险] .NET 10 + EFCore 8.x 兼容性** → .NET 运行时完全向后兼容低版本库
