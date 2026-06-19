## ADDED Requirements

### Requirement: 本地开发配置文件
系统 SHALL 提供 `docker/docker-compose.dev.yml` 配置文件，用于本地开发环境。

#### Scenario: 一键启动完整开发环境
- **WHEN** 开发者执行 `docker compose -f docker/docker-compose.dev.yml up`
- **THEN** 启动 PostgreSQL、IdServer、Admin 三个服务

#### Scenario: 代码热重载
- **WHEN** 开发者修改 `src/` 目录下的代码文件
- **THEN** IdServer 和 Admin 服务自动重新编译并重启

### Requirement: 代码目录挂载
本地开发配置 SHALL 挂载宿主机的 `src/` 目录到容器中，支持实时代码修改。

#### Scenario: 源代码同步
- **WHEN** 开发者在宿主机修改 `src/MinGo.IdServer/Program.cs`
- **THEN** 容器内的文件立即更新

#### Scenario: 依赖恢复
- **WHEN** 开发者在宿主机添加新的 NuGet 包
- **THEN** 容器内自动执行 `dotnet restore`

### Requirement: 端口映射
本地开发配置 SHALL 将服务端口映射到主机，便于开发者访问。

#### Scenario: 访问 IdServer
- **WHEN** 开发者在浏览器访问 `http://localhost:5001`
- **THEN** 能够访问 IdServer 的 OIDC 端点

#### Scenario: 访问 Admin
- **WHEN** 开发者在浏览器访问 `http://localhost:5002`
- **THEN** 能够访问 Admin 管理界面

### Requirement: 环境变量配置
本地开发配置 SHALL 支持通过环境变量自定义配置。

#### Scenario: 自定义数据库配置
- **WHEN** 开发者设置 `POSTGRES_PASSWORD=custom密码`
- **THEN** PostgreSQL 使用自定义密码启动

#### Scenario: 自定义端口
- **WHEN** 开发者设置 `IDSERVER_PORT=6001`
- **THEN** IdServer 映射到主机的 6001 端口
