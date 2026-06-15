## Why

Gitea Actions 工作流 `build.yml` 将 dotnet 测试和 Docker 镜像构建耦合在同一个 workflow 中，导致 PR 创建时会触发 3 个 Docker 镜像的空转构建，浪费 Runner 资源且延长 Pipeline 耗时。需要按事件类型分离职责：

- **PR 提交**：只需代码验证（编译 + 测试），不应触发镜像构建
- **版本发布**：先验证代码，验证通过后才构建并推送镜像
- **手动触发**：快速重建镜像推送至 dev 环境，跳过测试

## What Changes

- 删除原有的单文件 `.gitea/workflows/build.yml`
- 新增 `.gitea/workflows/test.yml` — 仅在 `pull_request` 时触发，执行 `dotnet restore` → `dotnet build` → `dotnet test`
- 新增 `.gitea/workflows/release.yml` — 在 `release` 和 `workflow_dispatch` 时触发
  - `release`: 先跑 `test` job（restore → build → test），通过后跑 `docker` job（3 镜像构建+推送）
  - `workflow_dispatch`: 仅跑 `docker` job（跳过测试），镜像 tag 使用 `dev-sha-{commit 前 8 位}`

## Capabilities

### New Capabilities

- `ci-test-workflow`: 独立的 PR 代码验证工作流，仅执行 dotnet 构建和单元测试
- `ci-docker-workflow`: 独立的发布/手动触发工作流，按需构建并推送 Docker 镜像

### Modified Capabilities

无（CI/CD pipeline 变更，不涉及产品功能规格）

## Impact

- **改动范围**：仅 `.gitea/workflows/` 目录下的 workflow 配置文件
- **外部依赖**：无新增（沿用已有 Gitea Action Runner）
- **回滚安全**：恢复旧 `build.yml` 即可回退到原始行为
- **不可逆注意**：拆分后 PR 不再触发 Docker 构建——如需要 PR 阶段的镜像验证，后续需单独增加
