## Context

当前 IdServer 启动时自动执行 EF 迁移和种子数据，多节点部署存在竞争。需要通过项目拆分将迁移能力独立。

## Goals / Non-Goals

**Goals:**
- Configuration 共享模块抽取
- Migrations 类库支持自定义扩展表 + 设计时
- Migrator 控制台应用支持部署时统一迁移
- IdServer 运行时不再涉及迁移

**Non-Goals:**
- 不改变 SimpleIdServer 官方迁移内容
- 不改 Admin 项目
- 不改 docker-compose.yml

## Decisions

| # | 决策 | 选择 | 理由 |
|---|------|------|------|
| D1 | Configuration 位置 | 独立类库 `MinGo.Identity.Configuration` | IdServer/Migrator/Migrations 三方共用，消除配置重复 |
| D2 | 配置加载方式 | `ConfigurationHelper.BuildDbContextOptions(Type, connStr)` | 统一方法，三方调用一致 |
| D3 | 自定义扩展表 | `ExtStoreDbContext` 放在 Migrations 项目 | DesignTimeFactory 在同一项目内，dotnet ef 可直接发现 |
| D4 | Migrator 命令 | `migrate` / `script` 两个命令，System.CommandLine 解析 | 清晰，可扩展 |
| D5 | Seed 归属 | 保留在 Migrator，迁移完成后执行 | 与迁移同生命周期 |
| D6 | IdServer 改动 | 移除 `.EnableMigration()` 和 `app.SeedData()`，引用 Configuration | 最小化修改 |

## Risks / Trade-offs

| 风险 | 缓解 |
|------|------|
| SIS 版本升级后新迁移不会被自动应用 | Migrator 执行 `EnableMigration()` 等效逻辑，升级时运行一次即可 |
| DesignTimeFactory 需要读取 appsettings.json | 引用 Configuration 模块，统一读取逻辑 |
| Migrations 项目与 SIS 迁移程序集版本耦合 | 通过 Directory.Packages.props 统一管理版本 |
