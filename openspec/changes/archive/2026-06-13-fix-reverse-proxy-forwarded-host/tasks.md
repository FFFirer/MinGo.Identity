## 1. 配置变更

- [ ] 1.1 在 `src/MinGo.IdServer/Program.cs` 中，`SidServerSetup.ConfigureIdServer()` 调用之后、`var app = webApplicationBuilder.Build()` 之前，追加 `ForwardedHeadersOptions` 补充配置
- [ ] 1.2 在配置中设置 `options.ForwardedHeaders` 包含 `XForwardedFor | XForwardedProto | XForwardedHost`
- [ ] 1.3 在配置中添加 `options.KnownNetworks.Clear()` 和 `options.KnownProxies.Clear()` 以信任容器内网转发头
- [ ] 1.4 将补充配置包裹在 `if (identityServerConfiguration.IsForwardedEnabled)` 条件中

## 2. 验证

- [ ] 2.1 确认 IdServer 项目编译通过：`dotnet build src/MinGo.IdServer`
- [ ] 2.2 通过 OIDC Discovery 端点验证 issuer URL 使用域名而非内部地址：`curl https://openid.example.com/.well-known/openid-configuration | jq .issuer`
- [ ] 2.3 验证登录流程中 302 重定向的 `Location` 使用域名
- [ ] 2.4 验证 `IsForwardedEnabled = false` 时回退到原行为不受影响
