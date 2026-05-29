## ADDED Requirements

### Requirement: Migrator Console Application
系统 SHALL 提供 `MinGo.Identity.Migrator` 控制台应用，支持 `migrate` 和 `script` 两个命令。

#### Scenario: migrate command
- **WHEN** 运行 `dotnet run -- migrate`
- **THEN** 依次执行 SIS 官方迁移 → 自定义迁移 → Seed 数据

#### Scenario: script command
- **WHEN** 运行 `dotnet run -- script --output up.sql`
- **THEN** 生成合并的 SQL 脚本到 up.sql 文件

### Requirement: Unified Migration Execution
Migrator SHALL 在单一事务中依次：
1. 通过 `StoreDbContext` 执行 SimpleIdServer 官方迁移（调用 `DbContext.Database.Migrate()`）
2. 通过 `ExtStoreDbContext` 执行自定义迁移
3. 执行 IDataSeeder 种子数据

#### Scenario: All migrations applied
- **WHEN** `migrate` 命令完成
- **THEN** SIS 表 + 自定义扩展表均存在，种子数据就绪

### Requirement: Configuration Consistency
Migrator SHALL 通过 `MinGo.Identity.Configuration` 模块读取 `appsettings.json`，配置加载方式与 IdServer 完全一致。

#### Scenario: Same config as IdServer
- **WHEN** 比较 Migrator/appsettings.json 与 IdServer/appsettings.json 的 StorageConfiguration 节
- **THEN** 格式一致，均使用 ConfigurationHelper 解析

### Requirement: CPM Integration
Migrator 项目 SHALL 使用 CPM 管理包版本，引用 Configuration 和 Migrations 项目。

#### Scenario: CPM configured
- **WHEN** 检查 `.csproj`
- **THEN** 项目引用 + CPM 包引用
