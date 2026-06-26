## Why

docker-compose.dev.yml 和 docker-compose.prod.yml 中 migrator/migrate 与 idserver 两个 service 之间重复定义了 6 个完全相同的环境变量（StorageConfiguration__Type、StorageConfiguration__ConnectionString、Authority、AdminUiUrl、IsRealmEnabled、IsForwardedEnabled），维护成本高且易出现配置不一致。

## What Changes

- 在 docker-compose.dev.yml 和 docker-compose.prod.yml 中引入 YAML anchor `x-idserver-env`，抽取 6 个共享环境变量
- migrator/migrate service 通过 `<<: *idserver-env` 复用共享配置
- idserver service 通过 `<<: *idserver-env` 复用共享配置
- service-specific 的环境变量（如 ASPNETCORE_URLS、DOTNET_ENVIRONMENT）保留在原 service 定义中

## Capabilities

### New Capabilities
<!-- No new capabilities — this is a pure refactoring of existing compose files -->

### Modified Capabilities
<!-- No spec-level behavior changes — implementation detail only -->

## Impact

- **配置文件**：`docker/docker-compose.dev.yml`、`docker/docker-compose.prod.yml`
- **向后兼容**：`docker compose config` 展开 anchor 后与之前的环境变量完全一致，无行为变化
