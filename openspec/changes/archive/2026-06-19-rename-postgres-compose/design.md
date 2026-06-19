## Context

当前 `docker/docker-compose.yml` 只包含 PostgreSQL，但文件名与 `docker-compose.dev.yml` 等并列，容易混淆。

## Goals / Non-Goals

**Goals:**
- 使文件名更直观
- 保持向后兼容（通过文档更新）

**Non-Goals:**
- 不改变功能
- 不影响其他配置文件

## Decisions

### 1. 命名选择

**选择**: `docker-compose.postgres.yml`

**理由**:
- 明确标识是 PostgreSQL 配置
- 与其他文件命名风格一致（`docker-compose.*.yml`）
- 简洁明了

## Risks / Trade-offs

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|----------|
| 用户习惯旧名称 | 低 | 低 | 更新文档 |

## Migration Plan

1. 重命名文件
2. 更新 README.md
