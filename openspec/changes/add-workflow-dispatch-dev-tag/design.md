## Context

当前 CI/CD 定义在 `.gitea/workflows/build.yml` 中，使用 Gitea Act Runner。支持 push/tag/PR 触发。Docker 镜像构建 Job 在矩阵中为 3 个服务（idserver, admin, migrator）生成 tag。

当前 tag 逻辑：
- 总是生成 `identity-{service}:{sha}`
- tag push 时额外生成 `identity-{service}:{semver}` 和 `identity-{service}:latest`
- push 条件: `gitea.ref_type == 'tag' || gitea.ref_name == 'main' || gitea.ref_name == 'master'`

需要新增 `workflow_dispatch` 手动触发支持，并在手动触发时生成 `dev` tag。

## Goals / Non-Goals

**Goals:**
- 支持从 Gitea UI 手动触发 CI/CD pipeline
- 手动触发时 Docker 镜像生成 `dev` tag（除 sha tag 外）
- 手动触发时正常 push 镜像到 registry
- 不影响现有 push/tag/PR 触发的行为

**Non-Goals:**
- 不添加可配置的 workflow_dispatch 输入参数
- 不改动 tag push 时的 semver/latest 逻辑
- 不添加 Docker image 清理逻辑
- 不涉及代码或依赖变更

## Decisions

### 1. 如何检测手动触发

在 Gitea Act Runner 中，`workflow_dispatch` 事件会将 `gitea.event_name` 设为 `workflow_dispatch`。在 Docker tag 步骤中通过判断 `${{ gitea.event_name }}` 是否为 `workflow_dispatch` 来决定是否添加 `dev` tag。

**备选方案考虑：**
- 使用 `github.event_name` — Gitea 使用 `gitea.event_name`，与 GitHub Actions 兼容
- 使用 `inputs` 参数 — 简单场景不需要，方案 A 已确定直接加 `dev` 标签

### 2. Push 条件的处理

当前 push 条件为 `gitea.ref_type == 'tag' || gitea.ref_name == 'main' || gitea.ref_name == 'master'`。`workflow_dispatch` 触发时 ref_name 是触发源分支（通常是 main/master），所以该条件在从 main/master 手动触发时也会通过。但为了明确支持所有情况，需要额外补充 `gitea.event_name == 'workflow_dispatch'`。

### 3. Docker tag 生成逻辑扩展

```yaml
# 总是 SHA tag
TAGS="$REGISTRY/mingo/identity-{svc}:${{ gitea.sha }}"

# 手动触发时加 dev
if [[ "${{ gitea.event_name }}" == "workflow_dispatch" ]]; then
  TAGS="$TAGS,$REGISTRY/mingo/identity-{svc}:dev"
fi

# tag push 时加 semver + latest（现有逻辑不变）
if [[ "${{ gitea.ref_type }}" == "tag" ]]; then
  TAGS="$TAGS,$REGISTRY/mingo/identity-{svc}:${{ gitea.ref_name }}"
  TAGS="$TAGS,$REGISTRY/mingo/identity-{svc}:latest"
fi
```

## Risks / Trade-offs

- **[低风险] `dev` tag 会被重复覆盖** — 每次手动触发都会重写同一个 `dev` tag。这是预期的行为，dev 环境始终使用最新构建。
- **[低风险] 手动触发时如果选择非 main/master 分支** — push 条件可能不满足导致镜像不 push。建议在 Gitea UI 上从 main 或 master 分支触发。
- **[无风险] 不影响现有流程** — `workflow_dispatch` 是新增事件，不会影响 push/tag/PR 的触发路径。
