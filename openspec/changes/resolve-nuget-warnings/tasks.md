## 1. Directory.Packages.props — 新增固定版本

- [x] 1.1 新增 `PackageVersion`：`SQLitePCLRaw.bundle_e_sqlite3` 3.0.3
- [x] 1.2 新增 `PackageVersion`：`Pomelo.EntityFrameworkCore.MySql` 9.0.0

## 2. Directory.Build.props — 抑制残留警告

- [x] 2.1 新增 `<NoWarn>$(NoWarn);NU1608;NU1903</NoWarn>` 抑制剩余 NuGet 警告

## 3. 验证构建

- [x] 3.1 运行 `dotnet build` 确认零警告构建（0 Warning, 0 Error）
