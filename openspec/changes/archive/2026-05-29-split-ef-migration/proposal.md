## Why

当前 IdServer 的 EF 迁移完全嵌入在 `SidServerSetup.EnableMigration()` 中，启动时自动运行。多节点部署下每个节点都会尝试迁移，存在竞态风险。同时自定义扩展表无法通过现有迁移体系管理。需将迁移能力从运行时拆分出去。

## What Changes

- **新建** `MinGo.Identity.Configuration` 共享配置类库（从 IdServer 提取 StorageConfiguration）
- **新建** `MinGo.Identity.Migrations` 迁移类库（自定义扩展表 + DesignTimeFactory）
- **新建** `MinGo.Identity.Migrator` 控制台应用（部署时统一执行迁移 + seed）
- **修改** `MinGo.IdServer`：移除 EnableMigration、SeedData，引用 Configuration
- **保留** `MinGo.IdServer.Admin` 不变

## Capabilities

### New Capabilities
- `shared-configuration`: 抽取 StorageConfiguration/StorageTypes/ConfigurationHelper 为独立类库，所有项目统一引用
- `custom-migrations`: 自定义扩展表（ExtStoreDbContext）+ DesignTimeFactory + 迁移文件目录
- `migrator-tool`: Console App，统一执行 SIS 官方迁移 + 自定义迁移 + Seed 数据

### Modified Capabilities
- 无（初始能力）

## Impact

- 新建 3 个项目，修改 1 个项目
- 运行时不再自动迁移，改为部署时由 Migrator 完成
- SidServerSetup.cs 移除 .EnableMigration() 和 ConfigureDataseeder()
- Program.cs 移除 app.Services.SeedData()
- Configuration 模块被 IdServer/Migrator/Migrations 三方共用
