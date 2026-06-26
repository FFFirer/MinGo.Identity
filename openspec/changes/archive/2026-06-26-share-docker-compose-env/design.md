## Context

当前 docker-compose.dev.yml 和 docker-compose.prod.yml 中，migrator/migrate 和 idserver 两个 service 各自独立声明了 6 个相同的环境变量：

- `StorageConfiguration__Type`
- `StorageConfiguration__ConnectionString`
- `Authority`
- `AdminUiUrl`
- `IsRealmEnabled`
- `IsForwardedEnabled`

这导致：
1. 修改变量默认值需要改两个地方
2. 新增共享变量容易遗漏某个 service
3. 容易产生配置漂移（如 prod.yml 的 migrate 之前就缺少 Authority/AdminUiUrl 等）

## Goals / Non-Goals

**Goals:**
- 消除两个 service 之间的环境变量重复
- 确保 migrator 和 idserver 使用完全相同的共享配置
- 保持 Docker Compose 语法兼容性（v3.8+）

**Non-Goals:**
- 不改变环境变量名或语义
- 不修改非共享的 service-specific 变量（ASPNETCORE_URLS、DOTNET_ENVIRONMENT 等）
- 不涉及 admin service 的环境变量

## Decisions

**方案：YAML Anchor + Merge Key**

使用 Docker Compose 标准的 `x-` 扩展字段定义 anchor，通过 `<<:` merge key 引用：

```yaml
x-idserver-env: &idserver-env
  StorageConfiguration__Type: ${STORAGE_TYPE:-POSTGRE}
  ...

services:
  migrator:
    environment:
      DOTNET_ENVIRONMENT: Production
      <<: *idserver-env    # ← 合并共享变量
```

选择理由：
- **标准 Docker Compose 特性**：YAML anchor 是 Compose v3 原生支持，无需额外工具
- **展开后行为一致**：`docker compose config` 展开 anchor 后与手工定义完全相同
- **渐进式迁移**：各 service 的专属变量保留在自身定义中，不强行统一
- **已在 test.yml 中使用相同模式**：`openspec/changes/fix-migrator-docker-issues/proposal.md` 提到类似思路

不选方案：
- **.env 文件集中管理**：环境变量来源不透明，不同 service 可能引用不同值
- **Docker Compose extends**：已弃用的特性，不推荐

## Risks / Trade-offs

- **[低风险] YAML Anchor 不支持合并 override**：如果某 service 需要覆盖 anchor 中的某个值，必须先在 anchor 中移除该变量。但当前场景下所有共享变量在 migrator 和 idserver 中值完全一致，无此需求。
- **[低风险] 编辑器对 `<<:` 语法高亮支持不一**：不影响运行时行为，Compose 解析器正确处理。
