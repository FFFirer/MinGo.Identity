## Requirements

### Requirement: Admin Project from Template
系统 SHALL 使用 `dotnet new idserveradmin` 模板脚手架生成 `MinGo.IdServer.Admin` 项目。
模板生成的项目 SHALL 包含 Blazor Interactive Server 配置、IdentityServerAdminConfiguration、以及 OpenTelemetry 支持。

#### Scenario: Scaffold from template
- **WHEN** 运行 `dotnet new idserveradmin -n MinGo.IdServer.Admin`
- **THEN** 生成 `IdServerAdmin.csproj`、`Program.cs`、`Components/App.razor`、`IdentityServerAdminConfiguration.cs` 等文件

### Requirement: Central Package Management
Admin 项目的 NuGet 包版本 SHALL 通过 `Directory.Packages.props` 集中管理。

#### Scenario: CPM configured
- **WHEN** 检查 `MinGo.IdServer.Admin.csproj` 中的 `PackageReference`
- **THEN** 所有引用不包含 `Version` 属性

### Requirement: OIDC Connection to IdServer
Admin 项目 SHALL 通过 OIDC 连接到 IdServer（`https://localhost:5001`）进行身份认证。
客户端 ID 为 `SIDS-manager`，密钥为 `password`，Scopes 为 `openid profile role`。

#### Scenario: OIDC configured
- **WHEN** 检查 `appsettings.json`
- **THEN** `IdserverBaseUrl` 为 `https://localhost:5001`，`ClientId` 为 `SIDS-manager`，`ClientSecret` 为 `password`

### Requirement: Port Configuration
Admin SHALL 监听 `https://localhost:5002`。

#### Scenario: Launch on port 5002
- **WHEN** 运行 `dotnet run`
- **THEN** 应用监听 `https://localhost:5002`

### Requirement: Realm Support
Admin SHALL 启用 Realm 支持（`IsRealmEnabled: true`）。

#### Scenario: Realm enabled
- **WHEN** 检查 `Program.cs`
- **THEN** 调用 `adminBuilder.EnableRealm()` 当配置 `IsRealmEnabled` 为 true
