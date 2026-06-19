## Why

Migrator 在 Docker 测试环境中运行失败，存在两个问题：

1. **配置文件找不到**：`Program.cs` 使用 `Directory.GetCurrentDirectory()` 加载 `appsettings.json`，但在 Docker 中当前目录是 `/src`（仓库根目录），而配置文件在 `/src/src/MinGo.Identity.Migrator/appsettings.json`，导致 `FileNotFoundException`。

2. **NuGet 包版本冲突**：`Pomelo.EntityFrameworkCore.MySql 8.0.2`（SidFormBuilder 的传递依赖）要求 EF Core 8.x，但项目使用 EF Core 10.x，产生 NU1608 警告。

## What Changes

- **修改 `Program.cs` 配置加载逻辑**：将 `Directory.GetCurrentDirectory()` 改为 `AppContext.BaseDirectory`，使配置文件路径基于程序集位置而非工作目录
- **抑制 NU1608 警告**：在 `MinGo.Identity.Migrator.csproj` 中添加 `<NoWarn>NU1608</NoWarn>`，抑制 Pomelo 包版本冲突警告（已知问题，无法修复直到 Pomelo 10.x 发布）

## Capabilities

### Modified Capabilities
- `migrator-tool`: 修复配置加载路径，确保在 Docker 环境中正确找到 appsettings.json

## Impact

- **代码文件**：`src/MinGo.Identity.Migrator/Program.cs`
- **项目文件**：`src/MinGo.Identity.Migrator/MinGo.Identity.Migrator.csproj`
- **Docker 配置**：`docker/docker-compose.test.yml`（无需修改，修复后可正常工作）
- **向后兼容**：修复后，本地开发和 Docker 环境均可正常运行
