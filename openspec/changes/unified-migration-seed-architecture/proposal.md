## Why

Admin 项目访问 `/master/clients` 时 `RealmMiddleware` 向 IdServer 请求 token 失败 → NRE → 重定向 `/availablerealms` → signout 路径丢失 realm 前缀。根因是 SIDS-manager 客户端的 redirect URL 从未被配置。

当前 Migrator 手动重建 DI 容器，与 IdServer 的 `SidServerSetup.ConfigureIdServer()` 注册不一致：`ConfigureDataseeder()` 从未调用，after-deployment seeder 未注册、未执行。SIDS-manager 的 redirect URL 由 `ConfigureAdminWebsiteRedirectUrlsDataSeeder` 配置，但该 seeder 从未运行。

## What Changes

- **Migrator 重写**：改为 `WebApplication.CreateBuilder` + `SidServerSetup.ConfigureIdServer()`，复用与 IdServer 完全一致的 DI 注册
- **Migrator 执行 SeedData()**：统一执行 migrations + seeders，完成后退出（不监听端口）
- **IdServer 不执行 SeedData()**：数据库由 Migrator 预先准备，IdServer 启动即为可用状态
- **SidServerSetup 补全 `ConfigureDataseeder()`**：注册所有 after-deployment seeder，修复 `SeedAdministrationData` 的 `baseUrl` 参数
- **删除 Migrator 中手动注册 seeders 的代码**：改为由共用 DI 自动注册
- **`docker-compose.dev.yml` 添加 migrator 服务**：作为 idserver 的前置依赖

## Capabilities

### New Capabilities
- `migration-seed-execution`: Migrator 统一管理数据库迁移 + 种子数据，使用与 IdServer 完全相同的 DI 容器，执行 `SeedData()` 后退出
- `after-deployment-seeders`: 注册并执行官方模板的 after-deployment `IDataSeeder`（含 `ConfigureAdminWebsiteRedirectUrlsDataSeeder` 等 8 个 seeder），补全 SIDS-manager 的 redirect URL 配置

### Modified Capabilities

- *(none — 本变更新增能力，未修改现有 spec 的行为边界)*

## Impact

| 文件 | 影响 |
|------|------|
| `src/MinGo.Identity.Migrator/Program.cs` | 重写：改用 `WebApplication.CreateBuilder` + `SidServerSetup.ConfigureIdServer` + `SeedData()` |
| `src/MinGo.Identity.Migrator/MigratorService.cs` | 删除（逻辑移到 Program.cs） |
| `src/MinGo.Identity.Migrator/MinGo.Identity.Migrator.csproj` | 加 `ProjectReference → MinGo.IdServer` |
| `src/MinGo.IdServer/Conf/SidServerSetup.cs` | 加 `ConfigureDataseeder()` 调用；`SeedAdministrationData` 的 `baseUrl` 改从配置读取 |
| `src/MinGo.IdServer/Program.cs` | 确认无 `SeedData()` 调用 |
| `docker/docker-compose.dev.yml` | 添加 migrator 服务 |
