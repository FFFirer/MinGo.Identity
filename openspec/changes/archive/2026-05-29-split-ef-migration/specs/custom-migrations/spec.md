## ADDED Requirements

### Requirement: Custom Migrations Class Library
系统 SHALL 提供 `MinGo.Identity.Migrations` 类库项目，包含 ExtStoreDbContext、DesignTimeFactory、Migrations 目录。

#### Scenario: Project created
- **WHEN** 检查解决方案
- **THEN** 存在 `src/MinGo.Identity.Migrations/MinGo.Identity.Migrations.csproj`

### Requirement: ExtStoreDbContext
`ExtStoreDbContext` SHALL 继承 `DbContext`，包含自定义扩展表（如 `UserProfile`）。
OnModelCreating SHALL 使用 Fluent API 配置表名、主键、字段约束。

#### Scenario: ExtStoreDbContext defined
- **WHEN** 检查 `ExtStoreDbContext.cs`
- **THEN** `DbSet<UserProfile>` 存在，`UserProfile` 表名为 `user_profiles`

### Requirement: DesignTimeFactory
`DesignTimeFactory` SHALL 实现 `IDesignTimeDbContextFactory<ExtStoreDbContext>`。
CreateDbContext SHALL 从 `appsettings.json` 读取 `StorageConfiguration` 并调用 ConfigurationHelper 构建选项。

#### Scenario: Factory creates context
- **WHEN** 运行 `dotnet ef migrations add`
- **THEN** DesignTimeFactory 被自动调用，创建指向 PostgreSQL 的 ExtStoreDbContext

### Requirement: CPM Integration
Migrations 项目 SHALL 使用 CPM 管理包版本，引用 Configuration 项目。

#### Scenario: CPM configured
- **WHEN** 检查 `.csproj`
- **THEN** 引用 Configuration 项目 + CPM 包引用
