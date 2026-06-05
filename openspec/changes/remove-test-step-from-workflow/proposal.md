## Why

项目中不存在任何测试项目，但 CI/CD 流水线的 build job 中仍然包含 `dotnet test` 步骤。每次触发都会额外花费 10-30 秒扫描测试项目，徒增流水线耗时。移除这个步骤可以加速构建流程，同时不影响任何实际功能。

## What Changes

- 从 `.gitea/workflows/build.yml` 的 build job 中移除 `Run tests` 步骤（第 65-66 行）
- 更新流水线头部注释，去掉 `test` 阶段描述

## Capabilities

### New Capabilities

<!-- No new capabilities - this is purely a CI/CD cleanup change -->

### Modified Capabilities

<!-- No spec-level behavior changes - this is an internal pipeline optimization -->

## Impact

- 仅修改 `.gitea/workflows/build.yml` 一个文件
- Docker job 的 `needs: [build]` 依赖关系不受影响
- 不影响任何代码、依赖或环境
- 由于没有测试项目，此变更无功能性影响
