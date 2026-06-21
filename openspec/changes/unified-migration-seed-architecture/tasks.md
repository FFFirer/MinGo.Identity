## 1. Build & Verify Baseline

- [x] 1.1 `dotnet build` 确认当前所有项目编译通过
- [x] 1.2 `lsp_diagnostics` 检查 `SidServerSetup.cs`、`Program.cs`、`MigratorService.cs` 无诊断错误

## 2. SidServerSetup — 补全 ConfigureDataseeder + 修正 baseUrl

- [x] 2.1 `SidServerSetup.cs` 末尾添加 `ConfigureDataseeder(webApplicationBuilder, configuration)` 调用
- [x] 2.2 创建 `ConfigureDataseeder()` 方法，注册 9 个 `IDataSeeder`（2 BeforeDeployment + 7 AfterDeployment）
- [x] 2.3 `SeedAdministrationData` 的 redirectUrl 参数从 `string.Empty` 改为 `$"{configuration.AdminUiUrl}/signout-callback-oidc"`
- [x] 2.4 验证 `IdentityServerConfiguration` 类有 `AdminUiUrl` 属性

## 3. Migrator — 重写为 WebApplication.CreateBuilder + SeedData

- [ ] 3.1 `MinGo.Identity.Migrator.csproj` 添加 `<ProjectReference Include="..\MinGo.IdServer\MinGo.IdServer.csproj" />`
- [ ] 3.2 重写 `Program.cs`：`WebApplication.CreateBuilder(args)` + `SidServerSetup.ConfigureIdServer()` + `Build()` + `SeedData()`
- [ ] 3.3 删除 `MigratorService.cs`（逻辑移入 Program.cs）
- [ ] 3.4 保留 `ExtStoreDbMigrateService.cs` 和 `InitUserProfileDataSeeder.cs`（通过 SidServerSetup DI 自动注册）
- [ ] 3.5 `dotnet build` 确认 Migrator 编译通过

## 4. IdServer — 确认 SeedData 移除

- [ ] 4.1 检查 `MinGo.IdServer/Program.cs` 确认无 `app.Services.SeedData()` 调用
- [ ] 4.2 确认 `SidServerSetup.ConfigureIdServer()` 注册完整（含 EnableMigration、UseEfStore、ConfigureDataseeder）

## 5. docker-compose.dev.yml — 集成 Migrator

- [ ] 5.1 添加 migrator 服务：`image: mcr.microsoft.com/dotnet/sdk:10.0`，`command: dotnet run --project src/MinGo.Identity.Migrator`
- [ ] 5.2 migrator 设置 `depends_on: postgres: condition: service_healthy`
- [ ] 5.3 migrator 注入必要的环境变量（`StorageConfiguration__Type`, `StorageConfiguration__ConnectionString` 等）
- [ ] 5.4 idserver 的 `depends_on` 改为 `migrator: condition: service_completed_successfully`
- [ ] 5.5 验证完整启动顺序：postgres → migrator → idserver → admin

## 6. 最终验证

- [ ] 6.1 `dotnet build` 全项目编译通过
- [ ] 6.2 启动 postgres + migrator，确认 SeedData 无异常
- [ ] 6.3 启动 IdServer，确认 /master/.well-known/openid-configuration 可达
- [ ] 6.4 启动 Admin，确认 /master/clients 正常渲染，无重定向到 /availablerealms
