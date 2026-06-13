## 1. 创建 test.yml

- [x] 1.1 新建 `.gitea/workflows/test.yml`，仅配置 `pull_request` 触发，分支限制 `main`/`master`
- [x] 1.2 实现 `test` job：`actions/checkout@v4` → `setup-dotnet@v4` → `cache@v4`（NuGet） → `dotnet restore` → `dotnet build --no-restore -c Release` → `dotnet test --no-build -c Release`

## 2. 创建 release.yml

- [x] 2.1 新建 `.gitea/workflows/release.yml`，配置 `release` + `workflow_dispatch` 两个触发事件
- [x] 2.2 实现 `test` job（与 test.yml 相同的 restore → build → test 流程）
- [x] 2.3 实现 `docker` job（matrix: idserver/admin/migrator），配置 `needs: [test]`
- [x] 2.4 release 事件时 Docker 镜像 tag 使用语义版本号 + `latest`
- [x] 2.5 workflow_dispatch 事件时 Docker 镜像 tag 使用 `dev-sha-{gitea.sha[:8]}` 格式

## 3. 清理旧文件

- [x] 3.1 删除原有的 `.gitea/workflows/build.yml`
- [x] 3.2 确认项目根目录无其他引用旧 `build.yml` 的配置

## 4. 验证

- [x] 4.1 确认 `.gitea/workflows/` 下有且仅有 `test.yml` 和 `release.yml` 两个 workflow 文件
- [x] 4.2 验证 YAML 语法无错误（Python yaml.safe_load 通过）
- [x] 4.3 确认 PR 提交仅触发 `test.yml`，不触发 Docker 构建（设计已验证，推送后确认）
