## 1. 包依赖

- [x] 1.1 在 `MinGo.Identity.Migrator.csproj` 中添加 `SimpleIdServer.IdServer` NuGet 包引用

## 2. 新建 ExtStoreDbMigrateService

- [x] 2.1 在 `MinGo.Identity.Migrator` 项目中创建 `ExtStoreDbMigrateService.cs`
- [x] 2.2 实现 `IDbMigrateService` 接口，构造函数注入 `ExtStoreDbContext`
- [x] 2.3 `Migrate()` 方法调用 `_dbContext.Database.MigrateAsync(ct)`

## 3. 新建 InitUserProfileDataSeeder

- [x] 3.1 在 `MinGo.Identity.Migrator` 项目中创建 `InitUserProfileDataSeeder.cs`
- [x] 3.2 继承 `BaseAfterDeploymentDataSeeder`，构造函数注入 `ExtStoreDbContext`
- [x] 3.3 实现 `Name` 属性返回 `nameof(InitUserProfileDataSeeder)`
- [x] 3.4 实现 `Execute()` 方法：检查 `UserProfiles.AnyAsync()`，为空则插入初始种子数据

## 4. 改造 MigratorService

- [x] 4.1 在 `MigratorService.cs` 中添加 `using DataSeeder`、`using Microsoft.Extensions.DependencyInjection`
- [x] 4.2 重写 `SeedDataAsync()`: 构建 `ServiceCollection`，注册以下服务：
  - `AddEfStore()` 配置 SIS 存储（复用 `ConfigurationHelper.BuildDbContextOptions`）
  - `AddDbContext<ExtStoreDbContext>()` 配置扩展存储
  - `AddDataseeder()` 注册 `IDataMigrationService`
  - 13 个官方 `IDataSeeder` 通过 `AddTransient` 注册（不含 `AddMigrationsScopeDataSeeder`，需额外引用 `SimpleIdServer.IdServer.Migrations` 包）
  - `ExtStoreDbMigrateService` 通过 `AddTransient<IDbMigrateService>` 注册
  - `InitUserProfileDataSeeder` 通过 `AddTransient<IDataSeeder>` 注册
- [x] 4.3 `BuildServiceProvider()` → 调用 `sp.SeedData()`
- [x] 4.4 在 `SeedDataAsync()` 前后添加 `Console.WriteLine` 日志
- [x] 4.5 移除 `RunMigrationsAsync()` 中原三步独立流程
- [x] 4.6 `CreateSisDbContext()` 和 `CreateExtDbContext()` 保留，`GenerateScriptAsync` 仍使用

## 5. 验证

- [x] 5.1 `dotnet build` 通过
- [ ] 5.2 在空数据库上运行 `dotnet run -- migrate`，验证 Language 表有 en/fr 记录
- [ ] 5.3 再次运行 `dotnet run -- migrate`，验证幂等性（无重复数据）
- [ ] 5.4 验证 `DataSeederExecutionHistory` 表有所有 seeder 的执行记录
- [ ] 5.5 启动 `MinGo.IdServer`，验证不再抛出 "default language en" 异常
