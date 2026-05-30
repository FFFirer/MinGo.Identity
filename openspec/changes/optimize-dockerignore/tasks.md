## 1. 更新 .dockerignore

- [x] 1.1 增加 `openspec/` 整体排除规则（替换当前仅 `openspec/changes/archive/` 的规则）
- [x] 1.2 增加 `.gitea/` CI 配置排除规则
- [x] 1.3 增加 `nul` 异常文件排除规则
- [x] 1.4 增加 `*.binlog` MSBuild 二进制日志排除规则
- [x] 1.5 增加根目录 `*.md` 文档排除规则
- [x] 1.6 重新整理分组注释和规则排序，提高可维护性

## 2. 验证构建

- [x] 2.1 验证 `.dockerignore` 语法正确 ✅ 文件结构完整，分组清晰无歧义
- [ ] 2.2 ~~执行 `docker build -f docker/Dockerfile.idserver ..` 验证 IdServer 构建通过~~ ⛔ 当前环境无 Docker，需手动验证
- [ ] 2.3 ~~执行 `docker build -f docker/Dockerfile.admin ..` 验证 Admin 构建通过~~ ⛔ 同上
- [ ] 2.4 ~~执行 `docker build -f docker/Dockerfile.migrator ..` 验证 Migrator 构建通过~~ ⛔ 同上
- [ ] 2.5 ~~对比优化前后的构建上下文大小~~ ⛔ 同上
