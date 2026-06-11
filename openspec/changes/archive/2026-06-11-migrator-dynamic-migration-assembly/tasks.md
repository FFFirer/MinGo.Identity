## 1. ConfigurationHelper — 新增迁移程序集映射方法

- [x] 1.1 在 `ConfigurationHelper.cs` 中新增 `GetSisMigrationsAssembly(StorageTypes type)` 方法，返回 SIS 按存储类型对应的程序集名称
- [x] 1.2 在 `ConfigurationHelper.cs` 中新增 `GetFormBuilderMigrationsAssembly(StorageTypes type)` 方法，返回 FormBuilder 按存储类型对应的程序集名称

## 2. MigratorService — 切换为动态迁移程序集

- [x] 2.1 `SeedDataAsync()` 中 `AddEfStore` 调用的迁移程序集从硬编码改为 `ConfigurationHelper.GetSisMigrationsAssembly(_storageConfig.Type)`
- [x] 2.2 `SeedDataAsync()` 中 `AddDbContext<FormBuilderDbContext>` 调用的迁移程序集从硬编码改为 `ConfigurationHelper.GetFormBuilderMigrationsAssembly(_storageConfig.Type)`
- [x] 2.3 `CreateSisDbContext()` 中的迁移程序集改为 `ConfigurationHelper.GetSisMigrationsAssembly(_storageConfig.Type)`

## 3. Migrator.csproj — 补齐全部后端迁移程序集

- [x] 3.1 添加 `SimpleIdServer.IdServer.SqlServerMigrations` / `.MySQLMigrations` / `.SqliteMigrations`
- [x] 3.2 添加 `SidFormBuilder.SqlServerMigrations` / `.MySQLMigrations` / `.SqliteMigrations`
- [x] 3.3 移除多余的独立包引用（PostgreMigrations 已归入统一 ItemGroup）

## 4. ExtStoreDbContext — 抑制 PendingModelChangesWarning

- [x] 4.1 模型快照基于 Npgsql 生成，SQLite 等 provider 运行时类型映射不同导致警告
- [x] 4.2 在 `AddDbContext<ExtStoreDbContext>` 中配置忽略 `PendingModelChangesWarning`

## 5. SidServerSetup — 复用 ConfigurationHelper

- [x] 3.1 `ConfigureIdserverStorage()` 改为调用 `ConfigurationHelper.BuildDbContextOptions()` + `ConfigurationHelper.GetSisMigrationsAssembly()`
- [x] 3.2 `ConfigureFormbuilderStorage()` 改为调用 `ConfigurationHelper.BuildDbContextOptions()` + `ConfigurationHelper.GetFormBuilderMigrationsAssembly()`

## 7. 验证

- [x] 7.1 `dotnet build` 通过无新警告
- [x] 7.2 对比 PostgreSQL 场景迁移输出：重构前后行为一致（`GetSisMigrationsAssembly(POSTGRE)` 返回与旧硬编码完全相同的字符串，零行为差异）
- [x] 7.3 SQLite 迁移验证通过：所有 SIS + FormBuilder 表创建，15 个 seeder 执行成功，`user_profiles` 种子数据正确
- [x] 7.4 幂等性验证：重复运行无错误，seeder 自动跳过已执行项
