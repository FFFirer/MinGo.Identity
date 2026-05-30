## Context

当前 `.dockerignore` (40 行) 覆盖了 Git、构建产物、IDE、开发工具等基本规则，但缺少部分目录和文件的排除规则。项目结构包含 openspec/ 设计文档、.gitea/ CI 配置、MinGo.Identity.slnx 解决方案文件等非构建输入。

三个 Dockerfile 均使用 `context: ..`（项目根目录）构建，通过 `COPY . .` 将整个上下文传入构建阶段。任何没有被 `.dockerignore` 排除的文件都会进入构建上下文，增加 `docker build` 的上下文传输时间和 daemon 解压时间。

## Goals / Non-Goals

**Goals:**
- 补充缺失的排除规则，缩小 Docker 构建上下文体积
- 整理 `.dockerignore` 结构，增加分组注释提高可维护性
- 确保不影响任何 Dockerfile 的正常构建

**Non-Goals:**
- 不修改 Dockerfile 本身
- 不修改 `docker-compose.yml` 或 `docker/docker-compose.prod.yml`
- 不改变编译产物或运行时行为
- 不涉及 CI/CD 管线的修改

## Decisions

### 排除规则选择

| 规则 | 原因 | 风险 |
|------|------|------|
| `openspec/` | 设计文档和规范，不参与编译。当前只排除了 `archive/` | 低风险，构建完全不需要 |
| `.gitea/` | Gitea Actions CI 配置，可能包含访问令牌 | 安全风险，应当排除 |
| `nul` | 根目录存在名为 `nul` 的异常文件，Windows 保留名 | 必须排除（构建报错风险） |
| `*.binlog` | MSBuild 二进制日志，体积大，不参与编译 | 低风险，但需确认被 `COPY . .` 影响 |
| `*.md` | 根目录的 README 等文档不参与编译 | 低风险，但 Dockerfile `COPY . .` 包含它们 |

### 不排除的规则

| 规则 | 原因 |
|------|------|
| `Directory.Build.props` | Dockerfile 显式 COPY 用于 restore 阶段 |
| `Directory.Packages.props` | 同上 |
| `nuget.config` | 同上 |
| `src/` | 源代码目录，必须保留 |
| `docker/` | Dockerfile 和 docker-compose.prod.yml，构建入口 |
| `MinGo.Identity.slnx` | 解决方案文件，保留以便未来需要解决方案级构建 |

### 分组结构

重新组织为带注释的区块，参考 `.gitignore` 的分组风格：

```
# Group: Git
# Group: .NET build artifacts
# Group: NuGet
# Group: OpenSpec / docs
# Group: IDE and editor
# Group: CI/CD
# Group: OS files
# Group: Environment
```

## Risks / Trade-offs

- **[低风险] `*.md` 排除过于宽泛**：如果未来有构建脚本依赖 README.md 的内容，此规则可能影响。通过只排除根目录 `*.md`（非递归）来限制影响范围。
- **[低风险] `openspec/` 整体排除**：如果将来 Dockerfile 需要读取 openspec 元数据（概率极低），需修改此规则。当前无此需求。
- **[无风险] `nul` 排除**：Windows 保留名在 Linux 容器中不会出现，排除是安全操作。
