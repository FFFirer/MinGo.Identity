## Why

当前项目面向 PostgreSQL，但 csproj 和代码中残留了大量 MySQL/MySql 相关的包引用和代码路径。这些残留导致：
- NU1608 编译警告（Pomelo.EntityFrameworkCore.MySql 9.0.0 与 EF Core 10.0.9 版本约束冲突）
- 无意义的依赖膨胀和构建时间增加
- 代码可维护性下降——读者需要区分哪些路径是实际可用的

同时，项目中还存在约 86 处 CS860x 空值相关的编译警告，需要精确修复以消除噪音，使编译输出干净。

## What Changes

- 从 3 个 csproj 中移除 MySQL/MySql 相关的包引用
- 将代码中所有 MySQL/MySql 分支替换为 `throw new NotSupportedException()`，原代码注释保留
- 精确修复所有 CS8600/CS8601/CS8602/CS8604 空值警告（Razor 视图）
- 修复 CS0618 已过时 API 警告（`AddSqliteCache`）
- **不**添加任何 `NoWarn` 抑制

## Capabilities

### New Capabilities
<!-- No new capabilities — this is a removal/cleanup change -->

### Modified Capabilities
<!-- No spec-level requirement changes — this is an implementation-only cleanup -->

## Impact

- **3 个项目文件**修改包引用
- **6 个 C# 文件**修改 MySQL 代码路径
- **~30 个 cshtml 文件**修复空值警告
- 消除了 NU1608、CS8600、CS8601、CS8602、CS8604、CS0618 共约 118 个警告
