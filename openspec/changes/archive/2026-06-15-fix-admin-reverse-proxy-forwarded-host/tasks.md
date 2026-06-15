## 1. 配置项新增

- [ ] 1.1 在 `IdentityServerAdminConfiguration.cs` 中新增 `IsForwardedEnabled` 属性（`bool`，默认 `false`）

## 2. ForwardedHeaders 配置

- [ ] 2.1 在 `Program.cs` 的 `Build()` 之前，追加 `Configure<ForwardedHeadersOptions>` 配置，设置 `ForwardedHeaders = XForwardedFor | XForwardedProto | XForwardedHost`
- [ ] 2.2 在同配置中调用 `KnownNetworks.Clear()` 和 `KnownProxies.Clear()`
- [ ] 2.3 将上述配置包裹在 `if (idserverAdminConfiguration.IsForwardedEnabled)` 条件中

## 3. 中间件注册

- [ ] 3.1 在 `Program.cs` 的 `Build()` 之后、`UseIdserverAdmin()` 之前，添加 `if (IsForwardedEnabled) app.UseForwardedHeaders();`

## 4. 配置文件更新

- [ ] 4.1 更新 `appsettings.json` 添加 `IsForwardedEnabled: true`
- [ ] 4.2 更新 `appsettings.Docker.json` 添加 `IsForwardedEnabled: true`

## 5. Docker Compose 更新

- [ ] 5.1 更新 `docker/docker-compose.prod.yml` admin service 的环境变量，添加 `IsForwardedEnabled: ${IS_FORWARDED_ENABLED:-false}` 和 `ASPNETCORE_FORWARDEDHEADERS_ENABLED: ${IS_FORWARDED_ENABLED:-false}`

## 6. 验证

- [ ] 6.1 确认 Admin 项目编译通过：`dotnet build src/MinGo.IdServer.Admin`
- [ ] 6.2 确认 `IsForwardedEnabled = false` 时回退到原行为不受影响
