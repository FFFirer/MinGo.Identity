## Why

当前 Migrator 的默认 `appsettings.json` 配置为 SQLite，但生产环境使用 PostgreSQL。集成测试虽通过 `DbFixture` 使用 PostgreSQL，但 Migrator 默认行为与实际运行环境不一致。需要将 Migrator 的默认配置切为 PostgreSQL，并补充 `ConfigurationHelper` 的单元测试，确保测试体系以 PostgreSQL 为第一目标。

## What Changes

- **`src/MinGo.Identity.Migrator/appsettings.json`** — 默认存储从 SQLite 改为 PostgreSQL，与生产环境一致
- **`src/MinGo.Identity.Migrator/appsettings.sqlite.json`** — 保留为本地离线开发专用配置
- **`tests/MinGo.Identity.Migrator.IntegrationTests/appsettings.json`** — 已有 PostgreSQL 配置，无需改动
- **`tests/MinGo.Identity.UnitTests/`** — 新建单元测试项目，覆盖 `ConfigurationHelper.GetSisMigrationsAssembly()`、`GetFormBuilderMigrationsAssembly()`、`BuildDbContextOptions()` 的核心逻辑

## Capabilities

### New Capabilities

- `configuration-helper-unit-tests`: ConfigurationHelper 的单元测试，覆盖所有 storage type 的分支和边界（含 null 和异常）

### Modified Capabilities

*（无）*

## Impact

- **MinGo.Identity.Migrator**: `dotnet run -- migrate` 默认连接 PostgreSQL 而非 SQLite；本地开发可用 `--configuration sqlite` 或显式切换 `appsettings.sqlite.json`
- **测试**: 新增单元测试项目，不依赖外部数据库，CI 可快速执行
