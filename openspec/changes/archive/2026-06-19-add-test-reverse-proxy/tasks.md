## 1. Nginx 配置

- [x] 1.1 创建 `docker/nginx/` 目录
- [x] 1.2 创建 Nginx 配置文件 `nginx.conf`
- [x] 1.3 创建 Nginx 服务配置 `conf.d/default.conf`
- [x] 1.4 生成自签名证书（可选） - 跳过，使用 HTTP 模式

## 2. Docker Compose 配置

- [x] 2.1 修改 `docker/docker-compose.test.yml` 添加 Nginx 服务
- [x] 2.2 配置 Docker 网络和域名解析
- [x] 2.3 更新 IdServer 和 Admin 环境变量

## 3. 文档和验证

- [x] 3.1 更新 README.md 添加测试环境说明
- [x] 3.2 验证域名解析 - 需要手动配置 hosts 文件
- [x] 3.3 验证 HTTP 访问 (8080) - 需要启动 docker compose 测试
- [x] 3.4 验证 HTTPS 访问 (8443)（可选） - 跳过，使用 HTTP 模式
