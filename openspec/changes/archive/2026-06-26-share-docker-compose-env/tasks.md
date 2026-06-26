## 1. docker-compose.dev.yml — 抽取共享环境变量

- [x] 1.1 添加 `x-idserver-env` YAML anchor，定义 6 个共享变量
- [x] 1.2 migrator service 移除重复变量，改用 `<<: *idserver-env`
- [x] 1.3 idserver service 移除重复变量，改用 `<<: *idserver-env`

## 2. docker-compose.prod.yml — 抽取共享环境变量

- [x] 2.1 添加 `x-idserver-env` YAML anchor，定义 6 个共享变量
- [x] 2.2 migrate service 移除重复变量，改用 `<<: *idserver-env`
- [x] 2.3 idserver service 移除重复变量，改用 `<<: *idserver-env`
