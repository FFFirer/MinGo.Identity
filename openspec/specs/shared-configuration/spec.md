## ADDED Requirements

### Requirement: Shared Configuration Class Library
系统 SHALL 提供 `MinGo.Identity.Configuration` 类库项目，包含 StorageConfiguration 模型、StorageTypes 枚举、ConfigurationHelper 工具类。

#### Scenario: Project created
- **WHEN** 检查解决方案
- **THEN** 存在 `src/MinGo.Identity.Configuration/MinGo.Identity.Configuration.csproj`

### Requirement: ConfigurationHelper
`ConfigurationHelper` SHALL 提供 `BuildDbContextOptions(DbContextOptionsBuilder, StorageConfiguration, string migrationsAssembly)` 静态方法。
该方法 SHALL 根据 StorageConfiguration.Type 值选择对应的数据库提供者（Npgsql/SqlServer/MySql/Sqlite/InMemory）并配置迁移程序集。

#### Scenario: Build PostgreSQL DbContext options
- **WHEN** 调用 `BuildDbContextOptions(builder, pgConfig, "MyMigrations")`
- **THEN** `builder.UseNpgsql(connStr, o => o.MigrationsAssembly("MyMigrations"))` 被调用

### Requirement: CPM Integration
Configuration 项目 SHALL 使用 CPM（Directory.Packages.props）管理包版本。

#### Scenario: CPM configured
- **WHEN** 检查 `.csproj`
- **THEN** 所有 `PackageReference` 不包含 `Version` 属性
