## Why

当前项目有两个 docker-compose 配置：
- `docker-compose.yml`：仅包含 PostgreSQL，用于本地开发时手动启动服务
- `docker/docker-compose.prod.yml`：完整的生产配置，包含所有服务

缺少一个**本地开发+集成测试**的配置，用于：
1. 本地开发时一键启动完整环境（无需手动 `dotnet run`）
2. 集成测试时提供隔离的测试环境
3. CI/CD 中运行集成测试

## What Changes

- 新增 `docker/docker-compose.dev.yml`：本地开发配置，支持代码热重载
- 新增 `docker/docker-compose.test.yml`：集成测试配置，包含测试数据库和完整服务栈
- 更新 `docker-compose.yml`：可选，添加注释说明不同配置的用途

## Capabilities

### New Capabilities
- `local-dev-compose`: 本地开发 docker-compose 配置，支持代码挂载和热重载
- `integration-test-compose`: 集成测试 docker-compose 配置，提供隔离的测试环境

### Modified Capabilities

（无现有 spec 需要修改）

## Impact

- **新增文件**: `docker/docker-compose.dev.yml`, `docker/docker-compose.test.yml`
- **可选更新**: `docker-compose.yml`（添加注释）
- **开发体验**: 一键启动完整开发环境
- **测试能力**: 支持自动化集成测试
- **CI/CD**: 可集成到 GitHub Actions 工作流
