## Context

### Current State

Migrator 当前手动重建 DI 容器，注册了 14 个 SIS 内置 seeder + 5 个管理 seeder + 2 个自定义 seeder，但**未注册** after-deployment seeder（`ConfigureAdminWebsiteRedirectUrlsDataSeeder` 等 8 个）。同时 `SidServerSetup.ConfigureDataseeder()` 从未被调用。

```
当前 Migrator DI  vs  官方 SidServerSetup DI
========================================
✅ 14 个 SIS 内置 seeder     ✅ 14 个 SIS 内置 seeder
✅ 5 个管理 seeder           ✅ 5 个管理 seeder
✅ 2 个自定义 seeder         ✅ 8 个 after-deployment seeder
❌ 8 个 after-deployment    ← 缺失！
❌ ConfigureDataseeder()     ✅ ConfigureDataseeder()
```

IdServer 的 `Program.cs` 中 `app.Services.SeedData()` 被注释移除（迁移分离设计），但 Migrator 也未完整执行 seeders。

### 问题链

```
Migrator SeedData() 不完整 → SIDS-manager redirect URL 未配置
  → Admin RealmMiddleware 请求 token 失败
  → JsonObject.Parse(json)["access_token"] is null → NRE
  → catch → 返回空 realm 列表 → 重定向 /availablerealms
  → signout 时 realm cookie 不存在 → redirectUri 丢失前缀
```

## Goals / Non-Goals

**Goals:**

- Migrator 使用与 IdServer **完全一致的 DI 注册**（通过 `SidServerSetup.ConfigureIdServer`）
- Migrator 统一执行迁移 + 种子数据（`SeedData()`），完成即退出，不监听端口
- IdServer 启动时不执行 `SeedData()`，数据库已由 Migrator 准备
- `ConfigureDataseeder()` 被调用，所有 after-deployment seeder 注册并执行
- 开发工作流中 migrator 自动在 idserver 启动前运行

**Non-Goals:**

- 不改变官方 SimpleIdServer `SeedData()` 扩展方法的内部行为
- 不改变现有 IdServer 的 API 行为和路由
- 不涉及 IdServer 运行时动态重配
- 不涉及 Admin 项目的代码修改

## Decisions

### Decision 1: Migrator 使用 WebApplication.CreateBuilder 而非 ServiceCollection

**Choice:** Migrator 改用 `WebApplication.CreateBuilder(args)` + `SidServerSetup.ConfigureIdServer()`，而非当前手动 `new ServiceCollection()`。

**Rationale:**
- `SidServerSetup.ConfigureIdServer()` 以 `WebApplicationBuilder` 为参数，内部调用 `builder.AddSidIdentityServer()`、`builder.Services.AddXxx()` 等，无法用 `ServiceCollection` 直接替代
- `WebApplication.CreateBuilder` 自动读取 `appsettings.json` 和环境变量，与 IdServer 行为一致
- 不需要手动注册任何服务，完全复用 IdServer 的 DI 注册

**Alternatives Considered:**
- **ServiceCollection + 手动重建注册**：当前做法，但与 IdServer 注册不一致，且遗漏 after-deployment seeder
- **共享 Configuration 扩展方法**：将 `ConfigureIdServer` 参数改为 `IServiceCollection`，但 `AddSidIdentityServer()` 需要 `WebApplicationBuilder`

### Decision 2: SeedData() 在 Migrator 统一执行

**Choice:** Migrator 调用 `app.Services.SeedData()` 同时执行迁移和种子数据。IdServer 不调用。

**Rationale:**
- `SeedData()` 内部通过 `DataSeederExecutionHistoryRepository` 保证幂等，多次调用安全但无意义
- 分离后 IdServer 启动更快，不涉数据库写操作
- 多节点部署时仅 Migrator 做迁移，避免竞争

### Decision 3: ConfigureDataseeder() 加回 SidServerSetup

**Choice:** 在 `SidServerSetup.ConfigureIdServer()` 末尾恢复 `ConfigureDataseeder(webApplicationBuilder)` 调用。

**Rationale:**
- 这是官方模板的标准结构，所有 after-deployment seeder 通过此方法注册
- 8 个 after-deployment seeder 文件已存在于 `src/MinGo.IdServer/Conf/Migrations/`，仅缺注册调用
- 如此 Migrator 和 IdServer 共享同一注册路径

### Decision 4: SeedAdministrationData baseUrl 从配置读取

**Choice:** `SeedAdministrationData` 的 redirect URL 参数改为 `$"{config.AdminUiUrl}/signout-callback-oidc"`，而非当前 `string.Empty`。

**Rationale:**
- 空字符串导致 SIDS-manager 的 redirect URI 为空，影响 OIDC 流程
- `config.AdminUiUrl` 已有配置（默认 `https://localhost:5002`），可直接使用
- `ConfigureAdminWebsiteRedirectUrlsDataSeeder` 会进一步补充通配路径

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Migrator 引用 IdServer 项目，IdServer 启动时可能触发 Migrator 入口 | .NET 项目引用仅引用编译后的类型和 assembly，不触发入口点。`Program.cs` 的 `Main` 不会被自动执行 |
| `WebApplication.CreateBuilder` 读取 launchSettings 可能产生意外配置 | Migrator 设置 `ASPNETCORE_ENVIRONMENT=Development` 且通过 `appsettings.json` 显式配置，不依赖 launchSettings |
| SeedData 执行时间较长，docker-compose 启动顺序依赖 | migrator 单独服务，idserver 通过 `condition: service_completed_successfully` 等待 |
| 删除 `MigratorService.cs` 可能影响现有脚本调用 | 替换为新的 `Program.cs`，CLI 入口保持不变（`dotnet run --project src/MinGo.Identity.Migrator`） |

## Migration Plan

1. 修改 `SidServerSetup.cs` — 加 `ConfigureDataseeder()`，修正 `baseUrl` 参数
2. 修改 `MinGo.Identity.Migrator.csproj` — 加 `ProjectReference → MinGo.IdServer`
3. 重写 `Migrator/Program.cs` — `WebApplication.CreateBuilder` + `SidServerSetup.ConfigureIdServer` + `SeedData()`
4. 删除 `MigratorService.cs`（逻辑已移入 Program.cs）
5. 确认 `MinGo.IdServer/Program.cs` 无 `SeedData()` 调用
6. 更新 `docker/docker-compose.dev.yml` — 添加 migrator 服务

**回滚策略：** 保留 `MigratorService.cs` 的备份，恢复 csproj 引用，移除 docker-compose 变更。
