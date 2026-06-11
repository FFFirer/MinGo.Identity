## Context

当前测试和配置现状：

| 层 | 当前配置 | 问题 |
|----|---------|------|
| Migrator `appsettings.json` | SQLite | 默认运行 `migrate` 时使用 SQLite，与生产 PostgreSQL 不一致 |
| Migrator `appsettings.sqlite.json` | SQLite（重复） | 与默认配置完全一致，冗余 |
| 集成测试 `appsettings.json` | PostgreSQL ✅ | 正确 |
| 单元测试 | ❌ 不存在 | `ConfigurationHelper` 的业务逻辑（switch 分支、null 处理、异常）无测试覆盖 |

## Goals / Non-Goals

**Goals:**
- Migrator 默认 `appsettings.json` 切为 PostgreSQL
- 清理冗余的 `appsettings.json.bak` 和重复的 `appsettings.sqlite.json`
- 新建 `MinGo.Identity.UnitTests` 项目
- `ConfigurationHelper` 三个公开方法全覆盖：5 种 storage type 分支 + INMEMORY null 处理 + 非法值异常

**Non-Goals:**
- 不改动 `MigratorIntegrationTests` 的 PostgreSQL 配置（已有且正确）
- 不添加任何外部数据库依赖到单元测试

## Decisions

### 决策 1：默认 appsettings 使用 PostgreSQL 连接字符串

```json
{
  "StorageConfiguration": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mingo_identity;Username=postgres;Password=123456",
    "Type": "POSTGRE"
  }
}
```

本地开发仍可通过 `--configuration sqlite` 或手动指定 `appsettings.sqlite.json` 使用 SQLite。

### 决策 2：单元测试项目结构

```
tests/MinGo.Identity.UnitTests/
├── MinGo.Identity.UnitTests.csproj    <- xunit
├── Configuration/
│   ├── GetSisMigrationsAssemblyTests.cs
│   ├── GetFormBuilderMigrationsAssemblyTests.cs
│   └── BuildDbContextOptionsTests.cs
```

使用 `Theory` + `InlineData` 参数化测试所有 storage type 分支，无需 mock。

## Risks / Trade-offs

| 风险 | 缓解 |
|------|------|
| 默认改 PostgreSQL 后，本地无 PG 容器的开发者首次运行会报连接错误 | 保留 `appsettings.sqlite.json`，文档说明本地开发用法 |
| 单元测试中 `BuildDbContextOptions` 涉及 `UseSqlServer`/`UseNpgsql` 等，需对应 provider 包 | 测试项目引用 `MinGo.Identity.Configuration`，它已包含所有 EF provider 包 |
