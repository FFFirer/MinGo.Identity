## Why

`docker/docker-compose.yml` 文件名不够直观，无法一眼看出它只启动 PostgreSQL。重命名为 `docker-compose.postgres.yml` 更清晰。

## What Changes

- 重命名 `docker/docker-compose.yml` → `docker/docker-compose.postgres.yml`
- 更新 `README.md` 中的所有引用

## Capabilities

### New Capabilities

（无）

### Modified Capabilities

（无现有 spec 需要修改）

## Impact

- **文件重命名**: `docker/docker-compose.yml` → `docker/docker-compose.postgres.yml`
- **文档更新**: `README.md` 中的命令
