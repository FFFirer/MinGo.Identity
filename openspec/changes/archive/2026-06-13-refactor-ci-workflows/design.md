## Context

### 当前架构

当前所有 CI/CD 逻辑在单文件 `.gitea/workflows/build.yml` 中：

```yaml
# build.yml (当前)
on:
  pull_request:
    branches: [main, master]
  release:
    types: [published]
  workflow_dispatch:

jobs:
  build:        # dotnet restore → build（无 test 步骤）
  docker:       # 3 镜像构建 + 推送（needs: build）
```

问题：
1. `pull_request` 触发时 Docker 虽然不 push（有条件判断），但仍在空转构建
2. 没有 `dotnet test` 步骤，代码未经测试
3. 三种事件（PR/release/manual）的职责混在同一文件，触发条件需要嵌套 if 判断

### 目标状态

```
.gitea/workflows/
├── test.yml         # pull_request: 仅代码验证
└── release.yml      # release: test + docker | workflow_dispatch: 仅 docker
```

### 约束

- 使用 Gitea Actions（兼容 `actions/checkout@v4`、`actions/setup-dotnet@v4` 等标准 actions）
- Runner 运行在 `ubuntu-latest`（Linux 容器）
- Dockerfile 为自包含多阶段构建，不依赖主机 dotnet 环境
- 镜像 tag 需区分 release（semver）和 dev（sha 前缀）

## Goals / Non-Goals

**Goals:**

- PR 提交仅触发 dotnet restore → build → test，**不触发** Docker 镜像构建
- Release（版本发布）先跑 dotnet test，测试通过后构建并推送 3 个镜像
- 手动触发仅构建并推送 Docker 镜像，使用 `dev-sha-{前8位}` tag
- 保持与现有 Gitea/Act Runner 环境兼容，不引入新依赖
- 保留原有的 release tag 镜像命名方案（`{semver}`、`latest`）

**Non-Goals:**

- 不改动 Dockerfile 内容（构建逻辑保持不变）
- 不新增或修改测试项目结构
- 不涉及产品功能代码变更
- 不涉及部署环境配置

## Decisions

### 决策 1：拆分为两个 workflow 文件

| 方案 | 评价 |
|------|------|
| A. 单文件 + `if` 条件 | ❌ 事件/职责混叠，可读性差 |
| B. **拆分两文件**（选用） | ✅ 职责清晰，触发条件自然分离 |

**结论**：`test.yml` 管 PR 验证，`release.yml` 管发布/手动构建。

### 决策 2：两个 workflow 各自完整包含 dotnet restore/build

当前 `docker` job 的 Dockerfile 是自包含构建（内部有 `dotnet restore` + `dotnet publish`），不依赖外部的 `build` job 产出。因此两个 workflow 不需要共享 artifact：

```yaml
# release.yml
jobs:
  test:
    # dotnet restore → build → test（纯验证）
  docker:
    needs: [test]  # 仅在 release 事件时依赖 test
    # 内部自己 restore + publish
```

### 决策 3：workflow_dispatch 时 Docker tag 格式

| 场景 | Tag 格式 | 示例 |
|------|----------|------|
| `release` | `{semver}` + `latest` | `1.0.0`, `latest` |
| `workflow_dispatch` | `dev-sha-{commit:0:8}` | `dev-sha-a1b2c3d4` |

### 决策 4：测试步骤拆分

`dotnet test` 会隐式编译（`dotnet test` builds then runs），但为此分离 restore 步骤以利用缓存：

```yaml
- name: Cache NuGet packages
  # ...cache config...

- name: Restore dependencies
  run: dotnet restore

- name: Build all projects
  run: dotnet build --no-restore -c Release

- name: Run all tests
  run: dotnet test --no-build -c Release
```

使用 `--no-build` 跳过重复编译。

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 拆分后 PR 不再构建 Docker，如果 PR 有 Dockerfile 变更无法验证 | 合并后才发现 Docker 构建问题 | 在 Dockerfile 变更的 PR 中临时手动触发 `workflow_dispatch` 验证 |
| `test.yml` 和 `release.yml` 中 `test` job 重复定义 | 维护两份近似配置 | 改动频率低，可接受；若未来频繁调整可考虑复用 action |
| `workflow_dispatch` 没有 run dotnet test | 可能把坏代码打包进镜像 | 手动触发的典型场景是快速重建已有镜像（代码已通过 PR 验证），不适用于未验证代码的首次构建 |
