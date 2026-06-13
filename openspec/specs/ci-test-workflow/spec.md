## Purpose

Define the CI workflow that automatically validates code quality on Pull Requests. Ensures every PR to main/master branches passes dotnet restore, build, and test before merge — without triggering Docker image builds.

## Requirements

### Requirement: PR 提交自动触发代码验证

当 Pull Request 创建或更新时，CI SHALL 自动执行 dotnet 代码验证流程，确保代码质量和编译通过。

#### Scenario: PR 创建时自动触发测试

- **WHEN** 用户创建 Pull Request，目标分支为 `main` 或 `master`
- **THEN** CI SHALL 自动启动 `test` workflow
- **AND** 在 10 分钟内完成 dotnet restore → build → test 全过程

#### Scenario: PR 提交新 commit 时重新测试

- **WHEN** PR 已存在，用户推送新 commit
- **THEN** CI SHALL 重新触发 `test` workflow
- **AND** 利用 NuGet 缓存加速 restore

#### Scenario: 测试通过后不触发任何镜像构建

- **WHEN** `test` workflow 中的所有测试通过
- **THEN** CI SHALL 仅报告成功状态
- **AND** 不触发任何 Docker 镜像构建或推送

### Requirement: 验证步骤包含编译和单元测试

测试流程 SHALL 执行完整的 .NET 还原、编译和测试步骤，确保代码质量和兼容性。

#### Scenario: 完整的编译和测试流程

- **WHEN** `test` workflow 启动
- **THEN** CI SHALL 按顺序执行 `dotnet restore`、`dotnet build --no-restore -c Release`、`dotnet test --no-build -c Release`
- **AND** 任意步骤失败则 workflow 整体标记为失败
