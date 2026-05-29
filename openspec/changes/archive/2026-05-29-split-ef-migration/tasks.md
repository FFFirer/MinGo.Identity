## 1. MinGo.Identity.Configuration 共享模块

- [ ] 1.1 创建 `src/MinGo.Identity.Configuration/` 项目（classlib net10.0）
- [ ] 1.2 从 IdServer 移入 `StorageConfiguration.cs`、`StorageTypes.cs`、`Configurations/` 下相关文件
- [ ] 1.3 实现 `ConfigurationHelper.BuildDbContextOptions()` 静态方法
- [ ] 1.4 配置 CPM（移除 csproj 内联版本）
- [ ] 1.5 IdServer 引用 Configuration 项目，删除原有重复文件

## 2. MinGo.Identity.Migrations 迁移类库

- [ ] 2.1 创建 `src/MinGo.Identity.Migrations/` 项目（classlib net10.0）
- [ ] 2.2 实现 `ExtStoreDbContext`（示例：UserProfile 表）
- [ ] 2.3 实现 `DesignTimeFactory`（实现 IDesignTimeDbContextFactory，引用 Configuration 读取配置）
- [ ] 2.4 创建空 `Migrations/` 目录
- [ ] 2.5 配置 CPM + 项目引用

## 3. MinGo.Identity.Migrator 控制台应用

- [ ] 3.1 创建 `src/MinGo.Identity.Migrator/` 项目（console net10.0）
- [ ] 3.2 实现 `migrate` 命令（SIS 迁移 + 自定义迁移 + Seed）
- [ ] 3.3 实现 `script` 命令（生成 SQL 脚本）
- [ ] 3.4 添加 `appsettings.json`（与 IdServer 一致的格式）
- [ ] 3.5 配置 CPM + 项目引用

## 4. 修改 MinGo.IdServer

- [ ] 4.1 `SidServerSetup.cs` 移除 `.EnableMigration()` 调用
- [ ] 4.2 `SidServerSetup.cs` 移除 `ConfigureDataseeder()` 调用
- [ ] 4.3 `Program.cs` 移除 `app.Services.SeedData()`
- [ ] 4.4 验证 IdServer 启动后不再自动创建表

## 5. 更新解决方案与构建

- [ ] 5.1 更新 `MinGo.Identity.slnx` 添加 3 个新项目
- [ ] 5.2 `dotnet build` 通过
