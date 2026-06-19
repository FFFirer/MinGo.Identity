## ADDED Requirements

### Requirement: Nginx 反向代理配置
系统 SHALL 在集成测试环境中添加 Nginx 反向代理，通过域名访问服务。

#### Scenario: 通过域名访问 IdServer
- **WHEN** 开发者访问 `http://idserver.local:8080`
- **THEN** Nginx 将请求转发到 IdServer 服务

#### Scenario: 通过域名访问 Admin
- **WHEN** 开发者访问 `http://admin.local:8080`
- **THEN** Nginx 将请求转发到 Admin 服务

### Requirement: 自定义域名解析
系统 SHALL 配置 Docker 网络以支持自定义域名解析。

#### Scenario: Docker 网络配置
- **WHEN** 测试环境启动
- **THEN** Docker 网络配置 `idserver.local` 和 `admin.local` 域名解析

#### Scenario: 域名解析验证
- **WHEN** 在容器内执行 `ping idserver.local`
- **THEN** 解析到正确的 IP 地址

### Requirement: HTTP 端口访问
系统 SHALL 支持通过 8080 端口访问服务。

#### Scenario: HTTP 访问 IdServer
- **WHEN** 开发者访问 `http://idserver.local:8080`
- **THEN** 能够访问 IdServer 的 OIDC 端点

#### Scenario: HTTP 访问 Admin
- **WHEN** 开发者访问 `http://admin.local:8080`
- **THEN** 能够访问 Admin 管理界面

### Requirement: HTTPS 端口访问（可选）
系统 SHALL 支持通过 8443 端口访问 HTTPS 服务。

#### Scenario: HTTPS 访问 IdServer
- **WHEN** 开发者访问 `https://idserver.local:8443`
- **THEN** 能够访问 IdServer 的 OIDC 端点

#### Scenario: HTTPS 访问 Admin
- **WHEN** 开发者访问 `https://admin.local:8443`
- **THEN** 能够访问 Admin 管理界面

### Requirement: 环境变量配置
系统 SHALL 更新环境变量以使用新的域名和端口。

#### Scenario: IdServer 环境变量
- **WHEN** 测试环境启动
- **THEN** `Authority` 设置为 `http://idserver.local:8080`

#### Scenario: Admin 环境变量
- **WHEN** 测试环境启动
- **THEN** `IdserverBaseUrl` 设置为 `http://idserver.local:8080`

### Requirement: 健康检查
Nginx 服务 SHALL 包含健康检查配置。

#### Scenario: Nginx 健康检查
- **WHEN** 执行 `docker compose ps`
- **THEN** Nginx 服务显示为 `healthy` 状态
