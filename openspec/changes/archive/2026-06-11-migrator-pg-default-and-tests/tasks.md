## 1. Migrator 默认配置切为 PostgreSQL

- [x] 1.1 修改 `src/MinGo.Identity.Migrator/appsettings.json` 的 `StorageConfiguration.Type` 为 `POSTGRE`，ConnectionString 指向本地 PostgreSQL
- [x] 1.2 清理冗余的 `src/MinGo.Identity.Migrator/appsettings.json.bak`

## 2. 新建单元测试项目

- [x] 2.1 在 `tests/` 下创建 `MinGo.Identity.UnitTests` xunit 项目
- [x] 2.2 项目引用 `MinGo.Identity.Configuration`
- [x] 2.3 在 `tests/MinGo.Identity.UnitTests/Configuration/` 下创建测试文件

## 3. ConfigurationHelper 单元测试

- [x] 3.1 `GetSisMigrationsAssemblyTests.cs` — 6 个测试用例覆盖全部 storage type + 边界
- [x] 3.2 `GetFormBuilderMigrationsAssemblyTests.cs` — 6 个测试用例覆盖全部 storage type + 边界
- [x] 3.3 `BuildDbContextOptionsTests.cs` — 验证 null migrationsAssembly 不抛出异常

## 4. 验证

- [x] 4.1 `dotnet test tests/MinGo.Identity.UnitTests` — 14/14 通过
- [x] 4.2 `dotnet test tests/MinGo.Identity.Migrator.IntegrationTests` — 8/8 通过
- [x] 4.3 `dotnet build` — 0 errors
