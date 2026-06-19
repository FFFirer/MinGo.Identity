## Why

开发 MinGo.Identity 时，IdServer 和 Admin 两个服务需要同时运行才能完成端到端功能验证。当前需要手动打开两个终端分别执行 `dotnet run`，调试体验割裂且效率低。需要添加 VS Code compound launch 配置，实现 F5 一键同时启动两个服务，并支持同时断点调试。

## What Changes

- 创建 `.vscode/launch.json`，包含两个独立 launch configuration（IdServer、Admin）
- 添加 compound configuration `IdServer + Admin`，一键并行启动两个项目
- 保留现有 `src/**/Properties/launchSettings.json` 不变，launch.json 引用已有 profile
- Admin 启动时自动打开浏览器到 `https://localhost:5002`

## Capabilities

### New Capabilities
- `vscode-launch-config`: VS Code 调试启动配置，支持一键同时启动 IdServer 和 Admin

### Modified Capabilities

无。所有产品功能不变，仅增加开发者工具链配置。

## Impact

- 根目录新增 `.vscode/launch.json` 文件
- 不影响现有 `launchSettings.json` 配置
- 不影响任何业务代码
- 不影响 CI/CD 流程
- 对使用 Visual Studio 或 Rider 的开发者无影响（仅 VS Code 受益）
