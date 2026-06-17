## Context

当前两个 Dockerfile（idserver、admin）在 runtime 阶段安装 ICU globalization 依赖：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
RUN apk add --no-cache icu-libs icu-data-full
```

在中国大陆网络环境下，Alpine 官方 CDN `dl-cdn.alpinelinux.org` 的下载速度极不稳定，经常导致 `docker build` 失败。

## 可选方案对比

### 方案 A：RUN 内 sed 替换源（**选定方案**）

```dockerfile
RUN sed -i 's|dl-cdn.alpinelinux.org|mirrors.aliyun.com|g' /etc/apk/repositories \
    && apk add --no-cache icu-libs icu-data-full
```

### 方案 B：`--repository` 参数

```dockerfile
RUN apk add --no-cache \
    --repository https://mirrors.aliyun.com/alpine/v3.21/main \
    --repository https://mirrors.aliyun.com/alpine/v3.21/community \
    icu-libs icu-data-full
```

### 方案 C：BuildKit `--build-arg` 参数化

`docker-compose.prod.yml` 中传入构建参数 `APK_MIRROR`，Dockerfile 中通过 ARG 引用。

### 方案对比

| 维度 | A: sed | B: --repository | C: build-arg |
|------|--------|----------------|--------------|
| 实现复杂度 | 一行 | 两行（URL 需指定版本） | 多文件改动 |
| Alpine 版本耦合 | 无（替换的是 hostname） | 有（URL 含 v3.21） | 无 |
| 适用所有 apk 命令 | 是 | 仅当前 RUN | 是 |
| 境外 CI 构建影响 | 比官方慢 | 比官方慢 | 可切换（传不同参数） |
| 对用户透明 | 是 | 是 | 需配合 CI 配置 |

**选定方案 A**：

1. `sed` 替换 hostname 而非 URL path，自动适配任意 Alpine 版本（v3.20 / v3.21 / 未来 v3.22）
2. 一次替换，当前 RUN 和同层后续 apk 命令全部走镜像
3. 仅修改 Dockerfile 内部，无需外部参数或 CI 配合
4. 方案 C 虽然灵活性最高，但用户明确要求"写死在 Dockerfile 里"

## Goals / Non-Goals

**Goals:**
- 在中国大陆环境下，apk 包通过阿里云镜像下载，显著提升构建速度
- 修改 `docker/Dockerfile.idserver` 和 `docker/Dockerfile.admin`
- 保持镜像层数不变（sed + apk 合并为同一个 RUN）
- 改动对运行时无影响

**Non-Goals:**
- 不修改 `docker/Dockerfile.migrator`（该镜像无 apk add 操作，无需切换源）
- 不引入 `--build-arg` 参数化（用户要求硬编码）
- 不处理 docker `build` 阶段 SDK 镜像的 NuGet 源（那是 nuget.config 的职责）
- 不改动 docker-compose 或 CI 配置文件

## Decisions

### 决策 1：使用 `sed` hostname 替换而非 `--repository` 参数

`sed` 替换的是 `/etc/apk/repositories` 中的 hostname 部分，而非完整的 URL。

Alpine 仓库格式：
```
https://dl-cdn.alpinelinux.org/alpine/v3.21/main
https://dl-cdn.alpinelinux.org/alpine/v3.21/community
```

`sed` 只替换 `dl-cdn.alpinelinux.org` → `mirrors.aliyun.com`，路径 `v3.21` 保持不变，因此**不需要关注 Alpine 具体版本**。当微软升级 base 镜像到 v3.22 时，替换自动生效。

### 决策 2：合并到一个 RUN 指令

```dockerfile
RUN sed -i 's|dl-cdn.alpinelinux.org|mirrors.aliyun.com|g' /etc/apk/repositories \
    && apk add --no-cache icu-libs icu-data-full
```

理由：保持镜像层数不变。如果分开为两个 RUN，会多出一个只包含 sed 结果的中间层，浪费 ~12MB 镜像空间。

### 决策 3：不修改 migrator Dockerfile

migrator 的 runtime 阶段没有任何 `apk add`，加 sed 是 dead code。如果未来 migrator 需要 apk 包，届时再加。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| 阿里云镜像宕机或网络故障 | 阿里云内网镜像 SLA 极高。如出现，开发者可临时注释掉 sed 行回退官方源 |
| 境外 CI runner（非中国区）下载变慢 | 阿里云全球节点也覆盖较好，影响有限。用户明确要求硬编码，这是有意识的选择 |
| 阿里云镜像同步延迟导致包版本滞后 | Alpine 镜像通常 1 小时内同步。ICU 包版本不敏感，滞后无影响 |
| `sed` 替换格式有误 | `s|dl-cdn.alpinelinux.org|mirrors.aliyun.com|g` 中的 `|` 分隔符避免与 URL 中的 `/` 冲突，经验证稳定可靠 |
