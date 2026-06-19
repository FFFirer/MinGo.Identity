## ADDED Requirements

### Requirement: IdServer 使用 Slim 基础镜像
IdServer Dockerfile SHALL 使用 `mcr.microsoft.com/dotnet/aspnet:10.0-slim` 作为运行时基础镜像，并安装 `libgssapi-krb5-2` 和 `icu-libs` 依赖。

#### Scenario: IdServer 容器启动成功
- **WHEN** 构建并启动 IdServer 容器
- **THEN** 容器启动成功，无 `libgssapi_krb5.so.2` 缺失错误

#### Scenario: IdServer LDAP/Kerberos 功能正常
- **WHEN** IdServer 尝试进行 LDAP 或 Kerberos 认证
- **THEN** 认证功能正常工作，无原生库加载错误

### Requirement: Admin 使用 Slim 基础镜像
Admin Dockerfile SHALL 使用 `mcr.microsoft.com/dotnet/aspnet:10.0-slim` 作为运行时基础镜像，并安装 `icu-libs` 依赖。

#### Scenario: Admin 容器启动成功
- **WHEN** 构建并启动 Admin 容器
- **THEN** 容器启动成功，无依赖缺失错误

#### Scenario: Admin 管理功能正常
- **WHEN** 通过 Admin 界面管理用户和客户端
- **THEN** 所有管理功能正常工作

### Requirement: 配置阿里云 apt 换源
Dockerfile SHALL 配置 apt 使用阿里云镜像源，以优化国内网络环境下的构建速度。

#### Scenario: 构建过程中 apt 更新成功
- **WHEN** Docker 构建执行 `apt-get update`
- **THEN** 从阿里云镜像源成功更新包列表

#### Scenario: 依赖安装成功
- **WHEN** Docker 构建执行 `apt-get install`
- **THEN** 从阿里云镜像源成功安装所有依赖包

### Requirement: Migrator 保持 Alpine 基础镜像
Migrator Dockerfile SHALL 继续使用 `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` 作为运行时基础镜像，不添加额外依赖。

#### Scenario: Migrator 容器启动成功
- **WHEN** 构建并启动 Migrator 容器
- **THEN** 容器启动成功，能执行数据库迁移

#### Scenario: Migrator 镜像体积最小
- **WHEN** 构建 Migrator 镜像
- **THEN** 镜像体积约为 110MB（无额外依赖）

### Requirement: 镜像体积优化
使用 Slim 镜像后，IdServer 和 Admin 的总体积 SHALL 小于或等于使用 Alpine + ICU 的体积。

#### Scenario: IdServer 镜像体积对比
- **WHEN** 分别构建 Slim 和 Alpine 版本的 IdServer 镜像
- **THEN** Slim 版本体积 <= Alpine 版本体积

#### Scenario: Admin 镜像体积对比
- **WHEN** 分别构建 Slim 和 Alpine 版本的 Admin 镜像
- **THEN** Slim 版本体积 <= Alpine 版本体积
