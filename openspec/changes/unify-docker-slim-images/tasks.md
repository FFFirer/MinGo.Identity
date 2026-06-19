## 1. Dockerfile 变更

- [x] 1.1 修改 `docker/Dockerfile.idserver`：将基础镜像从 `aspnet:10.0-alpine` 改为 `aspnet:10.0-slim`
- [x] 1.2 修改 `docker/Dockerfile.idserver`：添加 apt 换源配置（阿里云镜像）
- [x] 1.3 修改 `docker/Dockerfile.idserver`：添加 `libgssapi-krb5-2` 和 `icu-libs` 依赖安装
- [x] 1.4 修改 `docker/Dockerfile.admin`：将基础镜像从 `aspnet:10.0-alpine` 改为 `aspnet:10.0-slim`
- [x] 1.5 修改 `docker/Dockerfile.admin`：添加 apt 换源配置（阿里云镜像）
- [x] 1.6 修改 `docker/Dockerfile.admin`：添加 `icu-libs` 依赖安装

## 2. 验证测试

- [ ] 2.1 本地构建 IdServer 镜像并验证启动成功
- [ ] 2.2 本地构建 Admin 镜像并验证启动成功
- [ ] 2.3 验证 IdServer 的 LDAP/Kerberos 功能正常
- [ ] 2.4 验证 Admin 的管理功能正常
- [ ] 2.5 对比镜像体积（Slim vs Alpine）
