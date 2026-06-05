## Why

当前 CI/CD 流程只支持 Push/Tag/PR 触发，无法在 Gitea UI 上手动触发构建。当需要构建开发版本进行测试或调试时，必须 push 或打 tag，不够灵活。增加 `workflow_dispatch` 支持后，可以在 Gitea 界面上随时手动触发构建，并自动生成 `dev` 标签的 Docker 镜像，方便部署 dev 环境。

## What Changes

- 在 `.gitea/workflows/build.yml` 的 `on:` 触发器中增加 `workflow_dispatch` 事件
- Docker 镜像标签逻辑中增加判断：当通过 `workflow_dispatch` 手动触发时，额外生成 `identity-{service}:dev` 标签
- 更新 Docker push 条件，确保手动触发时也能 push 镜像

## Capabilities

### New Capabilities

<!-- This is purely a CI/CD infrastructure change. No new product capability specs needed. -->

### Modified Capabilities

<!-- No existing specs are affected since this is a CI/CD configuration change, not a product requirement change. -->

## Impact

- 仅修改 `.gitea/workflows/build.yml` 一个文件
- 不涉及代码逻辑变更
- 不涉及依赖或环境变更
- 手动触发时 Docker 镜像会多一个 `dev` tag，不影响现有 tag 行为
