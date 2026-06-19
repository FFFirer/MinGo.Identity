## 1. 本地开发配置

- [x] 1.1 创建 `docker/docker-compose.dev.yml` 配置文件
- [x] 1.2 配置 PostgreSQL 服务（端口 5432）
- [x] 1.3 配置 IdServer 服务（代码挂载 + dotnet watch）
- [x] 1.4 配置 Admin 服务（代码挂载 + dotnet watch）
- [x] 1.5 创建 `docker/.env.dev` 环境变量文件

## 2. 集成测试配置

- [x] 2.1 创建 `docker/docker-compose.test.yml` 配置文件
- [x] 2.2 配置 PostgreSQL 服务（端口 5433，独立数据库）
- [x] 2.3 配置 IdServer 服务（健康检查 + 依赖管理）
- [x] 2.4 配置 Admin 服务（健康检查 + 依赖管理）
- [x] 2.5 创建 `docker/.env.test` 环境变量文件

## 3. 文档和验证

- [x] 3.1 更新 README.md 添加新配置说明
- [ ] 3.2 验证本地开发配置可正常启动
- [ ] 3.3 验证集成测试配置可正常启动
- [ ] 3.4 验证代码热重载功能
- [ ] 3.5 验证测试数据隔离
