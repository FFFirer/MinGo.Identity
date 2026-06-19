## ADDED Requirements

### Requirement: 集成测试配置文件
系统 SHALL 提供 `docker/docker-compose.test.yml` 配置文件，用于集成测试环境。

#### Scenario: 启动测试环境
- **WHEN** 执行 `docker compose -f docker/docker-compose.test.yml up`
- **THEN** 启动 PostgreSQL（测试）、IdServer、Admin 三个服务

#### Scenario: 独立测试数据库
- **WHEN** 测试环境启动
- **THEN** 使用独立的 PostgreSQL 实例（端口 5433）和数据库 `mingo_identity_test`

### Requirement: 测试数据隔离
集成测试配置 SHALL 使用独立的数据库，避免与开发/生产数据混淆。

#### Scenario: 数据库隔离
- **WHEN** 测试环境启动
- **THEN** PostgreSQL 使用 `mingo_identity_test` 数据库

#### Scenario: 端口隔离
- **WHEN** 测试环境启动
- **THEN** PostgreSQL 映射到主机的 5433 端口（避免与本地开发冲突）

### Requirement: 健康检查
集成测试配置 SHALL 包含健康检查，确保服务完全启动后才运行测试。

#### Scenario: 服务健康检查
- **WHEN** 执行 `docker compose -f docker/docker-compose.test.yml ps`
- **THEN** 所有服务显示为 `healthy` 状态

#### Scenario: 依赖启动顺序
- **WHEN** 测试环境启动
- **THEN** PostgreSQL 先启动，IdServer 等待 PostgreSQL 健康后启动，Admin 等待 IdServer 健康后启动

### Requirement: 测试数据初始化
集成测试配置 SHALL 支持数据库迁移和种子数据初始化。

#### Scenario: 自动迁移
- **WHEN** 测试环境启动
- **THEN** 自动执行数据库迁移

#### Scenario: 种子数据
- **WHEN** 测试环境启动
- **THEN** 初始化默认的管理员账户和测试客户端

### Requirement: CI/CD 集成
集成测试配置 SHALL 支持在 CI/CD 环境中运行。

#### Scenario: GitHub Actions 集成
- **WHEN** GitHub Actions 工作流执行集成测试
- **THEN** 使用 `docker-compose.test.yml` 启动测试环境

#### Scenario: 测试后清理
- **WHEN** 集成测试完成
- **THEN** 执行 `docker compose -f docker/docker-compose.test.yml down -v` 清理所有容器和数据
