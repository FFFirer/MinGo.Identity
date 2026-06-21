## Why

项目编译产生 NuGet 级别警告 16 条，包含两个问题：`SQLitePCLRaw.lib.e_sqlite3` 2.1.11 已知高危漏洞 (GHSA-2m69-gcr7-jv3q)，以及 `Pomelo.EntityFrameworkCore.MySql` 8.0.2 版本约束与项目 EF Core 10.x 不兼容导致的 NU1608。需要升级依赖并抑制不再适用的警告，消除编译噪音。

## What Changes

- 升级 `SQLitePCLRaw.bundle_e_sqlite3` 到 3.0.3（3.x 改用 `SourceGear.sqlite3`，消除漏洞链路）
- 升级 `Pomelo.EntityFrameworkCore.MySql` 到 9.0.0（最新版，但仍不兼容 EF Core 10.x）
- 在 `Directory.Build.props` 中抑制 NU1608 警告（Pomelo 暂无 EF Core 10 兼容版本）
- NU1903 随 SQLitePCLRaw 升级自动消除

## Capabilities

### New Capabilities

无新能力变更。

### Modified Capabilities

无已有能力规格变更。

## Impact

- `Directory.Packages.props`: 新增 2 个 `PackageVersion` 固定传递依赖版本
- `Directory.Build.props`: 新增 NU1608 抑制规则
- 所有引用 SQLite 的项目将不再引入有漏洞的 `SQLitePCLRaw.lib.e_sqlite3`
- `MinGo.IdServer` 的传递依赖 Pomelo 从 8.0.2 升级到 9.0.0
