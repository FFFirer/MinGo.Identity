## Context

项目使用 CPM (Central Package Management) 管理 NuGet 包版本，`CentralPackageTransitivePinningEnabled=true`。这意味着 Directory.Packages.props 中声明的版本会强制覆盖所有传递依赖的版本。

当前两个问题：
1. `Microsoft.EntityFrameworkCore.Sqlite` → `SQLitePCLRaw.bundle_e_sqlite3` 2.1.11 → `SQLitePCLRaw.lib.e_sqlite3` 2.1.11（有漏洞）
2. `Pomelo.EntityFrameworkCore.MySql` 8.0.2（SimpleIdServer.IdServer 的传递依赖）要求 EF Core Relational 8.x，但项目固定到 10.0.9

## Goals / Non-Goals

**Goals:**
- 消除 NU1903（SQLitePCLRaw 漏洞）— 通过升级到 SQLitePCLRaw 3.x
- 将 Pomelo 升级到最新 9.0.0
- 抑制 Pomelo 9.x 与 EF Core 10.x 不兼容导致的 NU1608（Pomelo 10.x 尚未发布）
- Build 输出零警告

**Non-Goals:**
- 不删除任何项目引用（不涉及重構项目结构）
- 不降级 EF Core（保持 10.x）
- 不等待 Pomelo 10.x 发布

## Decisions

### 1. SQLitePCLRaw 漏洞：固定 bundle_e_sqlite3 3.0.3

SQLitePCLRaw 2.1.x 已废弃，漏洞无法在 2.x 线修复。3.x 重构了包结构：
- `bundle_e_sqlite3` 3.0.3 不再依赖 `lib.e_sqlite3`（有漏洞的包）
- 改用 `SourceGear.sqlite3`（原生 SQLite 库，包含 SQLite 3.50.4+）

利用 `CentralPackageTransitivePinningEnabled=true`，只需在 Directory.Packages.props 中固定版本即可覆盖所有传递路径：

```
EF Core Sqlite 10.0.9
  └─ SQLitePCLRaw.bundle_e_sqlite3 (>= 2.1.x) → pinned 3.0.3
       └─ SourceGear.sqlite3 ✅ (无漏洞)
```

### 2. Pomelo：升级到 9.0.0 + 抑制 NU1608

Pomelo 9.0.0 要求 EF Core Relational >= 9.0.0 && <= 9.0.999，而 EF Core 10.0.9 超出上限。Pomelo 10.x（EF Core 10 兼容）尚未发布。

NU1608 在此场景是良性警告——Pomelo 在项目中只是 SimpleIdServer 模板代码的传递依赖，运行时使用 PostgreSQL，不会执行 Pomelo 代码。

### 3. NU1903 不需要 NoWarn

升级到 SQLitePCLRaw 3.0.3 后，`SQLitePCLRaw.lib.e_sqlite3` 不再被引用，NU1903 自动消失。

## Risks / Trade-offs

- **[低风险]** SQLitePCLRaw 3.x 与 EF Core Sqlite 10.0.9 的兼容性：EF Core Sqlite 对 bundle_e_sqlite3 只有最小版本约束（>= 2.1.x），3.0.3 兼容。且 SQLite 非运行时路径，即使有兼容问题也不影响生产。
- **[低风险]** Pomelo 9.0.0 与 SimpleIdServer 7.0.0 的兼容性：SimpleIdServer 内部可能使用 Pomelo 8.x API。但 Pomelo 9.x API 变化极小，且项目中 MySQL 代码从未被执行（仅在 DI 注册时可能引用类型），所以风险极低。
