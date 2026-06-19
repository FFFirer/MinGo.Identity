## Why

IdServer 容器启动时报告 `Error loading shared library libgssapi_krb5.so.2` 错误，原因是 Alpine 镜像缺少 Kerberos/GSSAPI 原生依赖。当前三个服务（IdServer、Admin、Migrator）都使用 Alpine 基础镜像，但 IdServer 因 SimpleIdServer 的 LDAP/Kerberos 集成需求，需要这些原生库。

解决方案是将 IdServer 改为 Debian slim 镜像并手动安装缺失依赖，同时为了统一维护，将 Admin 也改为 slim 镜像。Migrator 保持 Alpine（一次性 job，无需额外依赖）。

## What Changes

- **IdServer Dockerfile**: 基础镜像从 `aspnet:10.0-alpine` 改为 `aspnet:10.0-slim`，添加 `libgssapi-krb5-2` 和 `icu-libs` 依赖安装，配置 apt 换源至阿里云镜像
- **Admin Dockerfile**: 基础镜像从 `aspnet:10.0-alpine` 改为 `aspnet:10.0-slim`，添加 `icu-libs` 依赖安装，配置 apt 换源至阿里云镜像
- **Migrator Dockerfile**: 保持 `aspnet:10.0-alpine` 不变（无额外依赖需求）

## Capabilities

### New Capabilities
- `docker-slim-base`: 统一使用 Debian slim 作为 Docker 运行时基础镜像，包含依赖安装和换源配置

### Modified Capabilities

（无现有 spec 需要修改）

## Impact

- **文件变更**: `docker/Dockerfile.idserver`, `docker/Dockerfile.admin`
- **镜像体积**: IdServer 从 ~150MB 降至 ~90MB，Admin 从 ~150MB 降至 ~110MB
- **依赖**: 新增 `libgssapi-krb5-2`, `icu-libs` (Debian 版本)
- **构建时间**: 可能略增（apt 安装），但层缓存可优化
- **运行时**: glibc 替代 musl，原生库兼容性更好
