## 1. 脚手架新项目

- [ ] 1.1 备份并删除旧的 `src/MinGo.IdServer/` 和 `src/MinGo.IdServer.Admin/`
- [ ] 1.2 在 `src/` 下从模板脚手架 `MinGo.IdServer`（`dotnet new idserver`）
- [ ] 1.3 在 `src/` 下从模板脚手架 `MinGo.IdServer.Admin`（`dotnet new idserveradmin`）

## 2. 适配 CPM 和项目配置

- [ ] 2.1 将 `MinGo.IdServer.csproj` 的包引用版本号移除改为 CPM（Directory.Packages.props）
- [ ] 2.2 将 `MinGo.IdServer.Admin.csproj` 的包引用版本号移除改为 CPM
- [ ] 2.3 更新 `MinGo.IdServer.csproj`：添加 AssemblyName/RootNamespace/UserSecretsId
- [ ] 2.4 更新 `MinGo.IdServer.Admin.csproj`：添加 AssemblyName/RootNamespace/UserSecretsId

## 3. 精简包引用（注释 + 说明）

- [ ] 3.1 注释 IdServer csproj 中不需要的 SQL Server / MySQL / SQLite 迁移包
- [ ] 3.2 注释不需要的认证方式包（SAML/WsFed/FIDO2/VP/Mobile 等）
- [ ] 3.3 注释不需要的外部 IdP（Facebook/Google/Negotiate）
- [ ] 3.4 注释不需要的基础设施包（MassTransit/Duende/Openiddict/SCIM/LDAP 等）
- [ ] 3.5 保留核心包：PostgreMigrations、Store.EF、Pwd、Email、Sms、Otp、Swagger、Hangfire.PostgreSql 等

## 4. 配置 PostgreSQL 存储

- [ ] 4.1 更新 `appsettings.json`：改 `StorageConfiguration.Type` 为 `POSTGRE`，设连接字符串
- [ ] 4.2 更新 `Program.cs`：保持 `SidServerSetup.ConfigureIdServer()` 调用不变
- [ ] 4.3 验证 `SidServerSetup.cs` 的 `ConfigureIdserverStorage` 中 PostgreSQL 分支正确

## 5. 移植自定义种子数据

- [ ] 5.1 创建 `Conf/Migrations/AfterDeployment/AdminUserDataSeeder.cs`
- [ ] 5.2 在 `SidServerSetup.ConfigureDataseeder()` 中注册 `AdminUserDataSeeder`

## 6. 配置 Admin 项目

- [ ] 6.1 更新 Admin `appsettings.json`：匹配当前的 OIDC 配置（IdserverBaseUrl/ClientId/Scopes）
- [ ] 6.2 验证 Admin `Program.cs` 中的 `UpdateOpenid` 和 `EnableRealm` 配置
- [ ] 6.3 更新 Admin `launchSettings.json`：端口 5002

## 7. 更新解决方案文件

- [ ] 7.1 更新 `MinGo.Identity.slnx` 引用新的两个项目

## 8. 验证构建和运行

- [ ] 8.1 `dotnet build` 通过，无错误
- [ ] 8.2 启动 PostgreSQL 容器并运行 IdServer，验证 `/master` 登录页正常
- [ ] 8.3 运行 Admin，验证能通过 OIDC 连接到 IdServer
