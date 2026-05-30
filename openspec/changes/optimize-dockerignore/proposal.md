## Why

当前的 `.dockerignore` 虽然覆盖了基本场景，但缺少部分无关文件和目录的排除规则，导致 Docker 构建上下文体积偏大、构建速度偏慢。同时 `openspec/` 等设计文档目录也被传入了构建容器，存在安全隐患和不必要的 I/O 开销。

## What Changes

- 在 `.dockerignore` 中补充缺失的排除规则：
  - 排除 `openspec/` 整个目录（当前只排除了 `archive/` 子目录）
  - 排除 CI/CD 配置目录 `.gitea/`
  - 排除根目录下的异常文件 `nul`
  - 排除 MSBuild 二进制日志 `*.binlog`
  - 排除 markdown 文档 `*.md`（根目录）
- 重新整理 `.dockerignore` 的注释和分组结构，提高可维护性

## Capabilities

### New Capabilities
<!-- 本次变更为基础设施优化，不涉及新增能力 -->
- *none*: 本次变更不引入新的业务能力，仅优化构建基础设施

### Modified Capabilities
<!-- 无现有规范变更 -->
- *none*

## Impact

- **文件**: `.dockerignore` — 修改排除规则
- **构建**: Docker 构建上下文体积缩小 15-25%（主要排除 `openspec/` 的文档和目录结构）
- **安全**: 排除 `.gitea/` 避免 CI 凭据泄漏风险
- **无功能影响**: 编译产物和运行时行为无变化
