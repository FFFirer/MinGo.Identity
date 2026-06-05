## Context

当前 `.gitea/workflows/build.yml` 的 `build` job 包含 4 个步骤：
1. Checkout repository
2. Setup .NET SDK
3. Cache NuGet packages
4. Restore dependencies
5. Build all projects
6. **Run tests** ← 要移除

项目中没有测试 `.csproj` 文件，`dotnet test` 执行后会因找不到测试项目而退出（已被 `|| echo "No test projects found, skipping."` 处理）。但每次仍会额外消耗约 10-30 秒的项目扫描时间。

移除后 Docker job 的 `needs: [build]` 和 job 命名不受影响。

## Goals / Non-Goals

**Goals:**
- 移除 `build` job 中的 `Run tests` 步骤
- 更新流水线头部注释以反映新的阶段
- 加速 CI/CD 流程（减少 10-30 秒无意义等待）

**Non-Goals:**
- 不改动 job 结构或依赖关系
- 不改动 Docker 构建、tag、push 逻辑
- 不涉及代码或依赖变更

## Decisions

### 直接删除，而非注释

`dotnet test || echo "skipping"` 这种兜底写法就是"因为没有测试项目所以跳过"的权宜之计。直接删除最干净，后续需要测试时可以单独加回来。

### 不改动 job 名称

`build` job 的 `name: Build and Test` 本应改为 `Build`，但考虑：
- 对流水线执行无实质影响
- 显示名不是 API 契约
保持不动，减少无关变更。

## Risks / Trade-offs

- **[低风险] 后续加测试要手动加回步骤** — 这是预期行为，加测试时自然需要调整 CI
- **[无风险] Docker 构建不受影响** — `needs: [build]` 依赖的是 job，不是 step
