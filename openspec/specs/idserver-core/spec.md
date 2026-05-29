## Requirements

### Requirement: IdServer Project from Template
系统 SHALL 使用 `dotnet new idserver` 模板脚手架生成 `MinGo.IdServer` 项目。
模板生成的项目 SHALL 包含 SidServerSetup 统一配置类、完整的 Areas/Views/Resources UI 层、以及多存储后端支持。

#### Scenario: Scaffold from template
- **WHEN** 运行 `dotnet new idserver -n MinGo.IdServer`
- **THEN** 生成 `MinGo.IdServer.csproj`、`Program.cs`、`SidServerSetup.cs`、`IdentityServerConfiguration.cs` 等文件

### Requirement: PostgreSQL Storage
系统 SHALL 使用 PostgreSQL 作为持久化存储。
`appsettings.json` 中的 `StorageConfiguration.Type` SHALL 设为 `POSTGRE`，`ConnectionString` SHALL 指向本地 PostgreSQL 16 实例。

#### Scenario: Storage configured for PostgreSQL
- **WHEN** 系统启动读取 `appsettings.json`
- **THEN** `StorageConfiguration.Type` 等于 `POSTGRE` 且 `ConnectionString` 格式为 `Host=localhost;Port=5432;Database=mingo_identity;Username=postgres;Password=***`

### Requirement: Central Package Management
所有 NuGet 包版本 SHALL 通过 `Directory.Packages.props` 集中管理。
脚手架生成的 `.csproj` 中 SHALL 移除内联版本号改为 CPM 引用。

#### Scenario: CPM configured
- **WHEN** 检查 `MinGo.IdServer.csproj` 中的 `PackageReference`
- **THEN** 所有引用不包含 `Version` 属性，版本定义在 `Directory.Packages.props` 中

### Requirement: Commented-out Package References
不必要的包引用 SHALL 以注释形式保留（而非删除），并附中文说明。
注释内容包括但不限于：MySQL、SQLite、SQLServer、SAML、WsFederation、FIDO2、LDAP、SCIM、Duende Migration、Openiddict Migration、Facebook/Google 认证等。

#### Scenario: Unnecessary packages commented
- **WHEN** 检查 `MinGo.IdServer.csproj`
- **THEN** 不需要的包引用以 XML 注释 `<!-- ... -->` 包裹，注释中包含中文说明

### Requirement: Custom Admin Seed Data
系统 SHALL 在启动时通过 `IDataSeeder` 接口创建默认管理员用户（administrator/password）。
种子数据 SHALL 集成到模板的 `Conf/Migrations/AfterDeployment/` 体系中。

#### Scenario: Admin user seeded on first run
- **WHEN** 系统首次启动且数据库中不存在 `administrator` 用户
- **THEN** 创建用户 `administrator`、密码 `password`、邮箱 `admin@mingo.local`

### Requirement: Port Configuration
IdServer SHALL 监听 `https://localhost:5001`。
启动配置通过 `Properties/launchSettings.json` 设定。

#### Scenario: Launch on port 5001
- **WHEN** 运行 `dotnet run`
- **THEN** 应用监听 `https://localhost:5001`
