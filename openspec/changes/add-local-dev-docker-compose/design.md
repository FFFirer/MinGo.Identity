## Context

当前项目有两个 docker-compose 配置：
- `docker-compose.yml`：仅 PostgreSQL，用于本地开发
- `docker/docker-compose.prod.yml`：完整生产配置

开发者需要：
1. 本地开发时一键启动完整环境，无需手动 `dotnet run`
2. 集成测试时提供隔离的测试环境
3. CI/CD 中运行集成测试

## Goals / Non-Goals

**Goals:**
- 提供本地开发配置，支持代码热重载
- 提供集成测试配置，支持自动化测试
- 保持与生产配置的一致性
- 简化开发环境搭建

**Non-Goals:**
- 不替换现有的 `docker-compose.yml`
- 不修改生产配置
- 不引入新的测试框架

## Decisions

### 1. 文件位置和命名

**选择**: `docker/docker-compose.dev.yml` 和 `docker/docker-compose.test.yml`

**理由**:
- 与生产配置放在一起，便于维护
- 清晰区分不同用途
- 符合 docker-compose 多文件惯例

**替代方案**:
- 放在根目录 → 会与现有 `docker-compose.yml` 混淆
- 使用 profile → 增加复杂度

### 2. 本地开发配置设计

**选择**: 使用 `dotnet watch` + 代码挂载

**特性**:
- 挂载 `src/` 目录到容器
- 使用 `dotnet watch` 实现热重载
- 端口映射到主机
- 环境变量配置

**配置要点**:
```yaml
idserver:
  volumes:
    - ../src:/src
  command: dotnet watch --project src/MinGo.IdServer
```

### 3. 集成测试配置设计

**选择**: 使用独立的测试数据库 + 完整服务栈

**特性**:
- 独立的 PostgreSQL 实例（不同端口/数据库名）
- 完整的服务栈（PostgreSQL + IdServer + Admin）
- 健康检查和依赖管理
- 可选的数据清理

**配置要点**:
```yaml
postgres:
  ports:
    - "5433:5432"  # 避免与本地开发冲突
  environment:
    POSTGRES_DB: mingo_identity_test
```

### 4. 环境变量管理

**选择**: 使用 `.env` 文件 + 默认值

**理由**:
- 简化配置
- 支持不同环境覆盖
- 便于 CI/CD 集成

**文件**:
- `docker/.env.dev` - 本地开发默认值
- `docker/.env.test` - 集成测试默认值

## Risks / Trade-offs

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|----------|
| 端口冲突 | 中 | 服务无法启动 | 使用不同端口范围 |
| 热重载不工作 | 低 | 开发体验差 | 检查 Docker 文件挂载 |
| 测试数据污染 | 中 | 测试结果不可靠 | 使用独立数据库 |
| 镜像构建慢 | 低 | 开发效率低 | 使用多阶段构建缓存 |

## Migration Plan

1. **创建配置文件**: 新增 dev 和 test 配置
2. **本地测试**: 验证配置可正常工作
3. **文档更新**: 更新 README 说明新配置用途
4. **CI/CD 集成**: 在 GitHub Actions 中添加集成测试步骤

## Open Questions

1. 是否需要为 dev 配置添加 `watch` 支持？（.NET 8+ 支持 `dotnet watch`）
2. 是否需要为 test 配置添加数据清理机制？
3. 是否需要添加 `pgAdmin` 或 `Adminer` 等数据库管理工具？
