## Purpose

Define the CI workflow that builds and publishes Docker images on release events or manual triggers. Ensures that release builds first pass code validation, while manual workflow_dispatch triggers provide a fast path for dev image rebuilds with short-SHA tags.

## Requirements

### Requirement: 版本发布时先测试后构建

当发布版本时（Release），CI SHALL 先执行 `test` job 验证代码，测试通过后再构建并推送 3 个 Docker 镜像（idserver/admin/migrator），确保发出的镜像代码质量。

#### Scenario: Release 时先测试后构建

- **WHEN** 用户发布 Release（`release` 事件触发）
- **THEN** CI SHALL 先执行 `test` job（restore → build → test）
- **AND** `test` job 通过后 SHALL 执行 `docker` job
- **AND** 构建并推送 3 个镜像（`identity-idserver`、`identity-admin`、`identity-migrator`）

#### Scenario: Release 镜像 tag 使用语义版本号

- **WHEN** `release` 事件触发且 `gitea.ref_type` 为 `tag`
- **THEN** 镜像 tag SHALL 使用对应的语义版本号（如 `1.0.0`）
- **AND** SHALL 额外打 `latest` tag

#### Scenario: 测试失败时终止构建

- **WHEN** `test` job 中的任意测试失败
- **THEN** CI SHALL 中止整个 workflow
- **AND** 不执行 `docker` job，不构建任何镜像

### Requirement: 手动触发快速构建 dev 镜像

通过 `workflow_dispatch` 手动触发时，CI SHALL 跳过测试直接构建 Docker 镜像，使用 `dev-sha-{commit 前 8 位}` 作为镜像 tag，适用于快速重建和部署 dev 环境。

#### Scenario: 手动触发时跳过测试

- **WHEN** 用户手动触发 `workflow_dispatch`
- **THEN** CI SHALL 仅执行 `docker` job
- **AND** SHALL 不执行 `test` job

#### Scenario: 手动触发使用 dev 前缀 tag

- **WHEN** `workflow_dispatch` 触发且构建 Docker 镜像
- **THEN** 镜像 tag SHALL 使用 `dev-sha-{gitea.sha[:8]}` 格式
- **AND** SHALL 推送至容器仓库
