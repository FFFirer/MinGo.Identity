## Why

本地集成测试需要模拟生产环境的域名和端口配置。当前测试环境使用 `localhost:5001` 和 `localhost:5002`，无法验证生产环境的反向代理、域名解析和 HTTPS 配置。

通过添加 Nginx 反向代理，可以：
- 使用自定义域名（`idserver.local`、`admin.local`）
- 使用标准端口（8080/8443）
- 测试 HTTPS 配置
- 模拟生产环境的访问方式

## What Changes

- 添加 Nginx 反向代理配置
- 配置自定义域名解析
- 添加 HTTPS 支持（自签名证书）
- 修改 docker-compose.test.yml 添加 Nginx 服务
- 更新环境变量使用新域名

## Capabilities

### New Capabilities
- `test-reverse-proxy`: 集成测试环境的 Nginx 反向代理配置

### Modified Capabilities

（无现有 spec 需要修改）

## Impact

- **新增文件**: `docker/nginx/` 目录（配置文件和证书）
- **修改文件**: `docker/docker-compose.test.yml`
- **端口变更**: 8080 (HTTP), 8443 (HTTPS)
- **域名变更**: `idserver.local`, `admin.local`
