## Context

IdServer 启动时 DI 验证失败，错误为 `Unable to resolve service for type 'ILaunchMigrationService'`。`ConfigureMigration()` 已在 MassTransit 配置中调用并注册了 `LaunchMigrationConsumer`，但对应的服务注册 `.EnableMigration()` 从未执行。

SimpleIdServer v7.0.0 的迁移系统由两个扩展方法组成：

- `o.ConfigureMigration()` → 注册 MassTransit 消费者（在 `EnableMasstransit` 回调内调用）
- `.EnableMigration()` → 注册 `ILaunchMigrationService` 等服务（在 `IdServerBuilder` 链上调用）

当前代码只调用了前者，遗漏了后者。

## Goals / Non-Goals

**Goals:**
- 修复 IdServer 启动崩溃
- 注册 `ILaunchMigrationService` 及其依赖服务
- 启用基于 MassTransit 的迁移消息处理管线

**Non-Goals:**
- 不改变现有数据库迁移策略（仍由 `MinGo.Identity.Migrator` 独立管理）
- 不新增 NuGet 包引用
- 不改动配置文件或 appsettings

## Decisions

| 决策 | 选项 | 选择理由 |
|---|---|---|
| `.EnableMigration()` 插入位置 | 在 `.AddOpeniddictMigration()` 之后、`.AddPwdAuthentication()` 之前加入 | 与其他迁移相关扩展方法保持逻辑相邻；简单服务器迁移在启动流程中早于认证配置 |
| 是否需要新增包引用 | `SimpleIdServer.IdServer.Migrations` | 不需要——该包已通过 `AddDuendeMigration` / `AddOpeniddictMigration` 的传递依赖引入 |
| 是否对现有迁移分离策略产生影响 | 否 | `.EnableMigration()` 仅注册 DI 服务，EF Core 迁移仍通过 `MigratorService` 独立执行 |

## Risks / Trade-offs

- **[低风险]** `.EnableMigration()` 会注册 `AddMigrationsScopeDataSeeder`（一个 `IDataSeeder`）——该系统域已存在多个 data seeder，无冲突风险
- **[低风险]** `.EnableMigration()` 会注册 3 个迁移管理 API 路由——这些端点通过模板的标准授权机制保护，对外部调用无影响
- **[兼容性]** 启用迁移消费者后，若 MassTransit 消息队列中积累未消费的 `LaunchMigrationCommand`，启动时可能触发历史迁移执行——但当前消息队列为空（从未启用过该消费者），无此风险
