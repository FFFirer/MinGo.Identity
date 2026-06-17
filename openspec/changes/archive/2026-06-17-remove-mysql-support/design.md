## Context

MinGo.Identity 项目实际只使用 PostgreSQL 作为主存储，但代码中保留了 MySQL/MySql 的完整支持路径。这些代码来自 SimpleIdServer 模板的多后端设计，导致：

- 6 个 MySQL 相关包被引用，其中 Pomelo.EntityFrameworkCore.MySql 9.0.0 与 EF Core 10.0.9 版本约束冲突（NU1608）
- C# 代码中有 7 处 MySQL 分支路径（ConfigurationHelper、SidServerSetup、数据 Seeder）
- 枚举类型 StorageTypes 和 DistributedCacheTypes 仍保留 MYSQL 值，保证 switch 表达式的编译合法性

同时存在约 86 处 CS860x 空值警告，集中在 Razor 视图中来自 SimpleIdServer 模板代码的空值处理。

## Goals / Non-Goals

**Goals:**
- 消除所有 NU1608 包版本警告（移除 Pomelo 即可）
- 消除所有 CS860x 空值警告（精确修复，不添加 NoWarn）
- 消除 CS0618 过时 API 警告
- 运行时如果遇到 MySQL 配置，抛出 NotSupportedException 而不是静默失败

**Non-Goals:**
- 不修改 StorageTypes / DistributedCacheTypes 枚举定义（保留 MYSQL 值以保证编译通过）
- 不重构非 MySQL 相关代码
- 不添加 NoWarn 抑制任何警告

## Decisions

### 1. 移除包引用 vs 保留但标记
**决策**: 完全移除 6 个 MySQL/MySql 包引用。运行时不再能加载 MySQL 相关类型。

**理由**: 保留包引用但不用会继续触发 NU1608。移除是最干净的方案。

**备选**: 在 csproj 中将 MySQL 包条件化为 `<PackageReference Include="..." ExcludeAssets="all" />` 或 `<NoWarn>`——与用户要求相悖。

### 2. 枚举值保留 MYSQL
**决策**: `StorageTypes.MYSQL = 3` 和 `DistributedCacheTypes.MYSQL = 4` 保留不动。

**理由**: 这些枚举在 switch/switch 表达式中的值是合法的，删除它们需要修改所有 switch 添加兜底分支，引入不必要的代码变更。保留值但运行时抛异常更安全——配置为 MYSQL 时应用无法启动，而不是静默退化为其他行为。

### 3. CS860x 修复策略：null-forgiving 运算符优先
**决策**: 对 Razor 视图中确定不为 null 的表达式使用 `!` 运算符。

**理由**: 这些视图来自 SimpleIdServer 模板，在视图执行路径中：
- `HttpContextAccessor.HttpContext` 在 ASP.NET Core 请求管道内永远不会为 null
- `LayoutResource.ResourceManager.GetString()` 返回的字符串被立即用于控制逻辑，实际不会为 null
- `User.Identity` 在已认证分支中不会为 null

对有实际风险的（如 `Model.Template?.CssStyles`）使用条件 null 保护而不是 `!`。

### 4. CS0618: 使用 NeoSmart.Caching.Sqlite 命名空间直接调用
**决策**: 将 `AddSqliteCache()` 扩展方法替换为 `NeoSmart.Caching.Sqlite` 命名空间下的直接调用。

**理由**: 这是 SimpleIdServer 模板代码引用过时 API 的典型情况。同一个 using 已经存在于文件顶部（`NeoSmart.Caching.Sqlite.AspNetCore`），只需换到正确的扩展方法。

## Risks / Trade-offs

- **[低风险] MySQL 配置在运行时不可用**：如果配置中错误地设置了 MySQL 存储类型，应用启动时会抛 NotSupportedException。这比当前在 NU1608 警告下静默运行要好——用户会立即知道配置无效。
- **[低风险] 移除包后 git 历史中不再有引用**：如果将来需要 MySQL 支持，需要重新添加，但项目定位是 PostgreSQL-only，不太可能发生。
- **[无风险] cshtml 修复可能在 SimpleIdServer 版本升级时被覆盖**：视图模板来自 SimpleIdServer NuGet 包，版本升级时这些视图可能被刷新。不过和 SimpleIdServer 锁定了 7.0.0 版本，短期内不会变。
