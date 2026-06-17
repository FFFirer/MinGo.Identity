## Why

在中国大陆网络环境下，从 Alpine 官方源 `dl-cdn.alpinelinux.org` 下载 apk 包非常缓慢甚至超时，导致 Docker 构建失败或耗时过长。

当前 `docker/Dockerfile.idserver` 和 `docker/Dockerfile.admin` 的 runtime 阶段都通过 `apk add --no-cache icu-libs icu-data-full` 安装 ICU 依赖。在中国境内构建时，这个步骤经常因网络问题失败。

## What Changes

- `docker/Dockerfile.idserver` — 在 `apk add` 之前通过 `sed` 将 Alpine 包源替换为阿里云镜像 `mirrors.aliyun.com`
- `docker/Dockerfile.admin` — 同上

两个 Dockerfile 的改动完全一致：在 apk 安装前增加一行 `sed` 替换 `dl-cdn.alpinelinux.org` → `mirrors.aliyun.com`，与 `apk add` 合并到同一个 `RUN` 指令以保持镜像层数不变。

## Capabilities

### New Capabilities

- `china-apk-mirror`: Docker 构建在中国大陆环境下使用阿里云 Alpine 镜像源，加速 apk 包下载

### Modified Capabilities

None. 这是一个构建基础设施优化，不涉及运行时行为变更或 spec 级需求变更。

## Impact

- **Affected files**: `docker/Dockerfile.idserver`, `docker/Dockerfile.admin`
- **镜像层数**: 不变（sed + apk 合并在同一个 RUN 指令）
- **镜像大小**: 无影响
- **风险**: 低 — 阿里云镜像源稳定可靠。唯一副作用是境外 CI runner（如 GitHub Actions US）构建时比官方源稍慢，但在可接受范围内
