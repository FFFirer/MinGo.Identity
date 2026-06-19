## Context

当前 MinGo.Identity 开发工作流中，IdServer（端口 5001）和 Admin（端口 5002）需要同时运行。两个项目各自有独立的 `launchSettings.json` 文件，但缺少统一的启动入口。开发者需要手动维护两个终端窗口或依赖 IDE 的多启动项目配置。

团队主要使用 VS Code 进行开发，但项目根目录没有 `.vscode/` 目录和相关调试配置。

## Goals / Non-Goals

**Goals:**
- 创建 `.vscode/launch.json`，包含 IdServer 和 Admin 的独立 launch configuration
- 提供 compound configuration 实现 F5 一键同时启动两个项目
- 支持两个进程同时打断点调试
- 保留现有 `launchSettings.json` 作为 CLI 启动的兜底

**Non-Goals:**
- 不修改任何业务代码
- 不做端口/URL 自动发现（沿用 `launchSettings.json` 中已有的配置）
- 不支持 Visual Studio 或 Rider（它们各有自己的多启动项目机制）
- 不添加容器化启动方案（开发环境仍用 docker-compose 管理 PostgreSQL）
- 不处理服务启动顺序依赖（IdServer 和 Admin 启动独立，由开发者自行确保 IdServer 在 Admin 之前就绪）

## Decisions

### 1. 使用 coreclr launch type 而非 dotnet 命令行

- **选择**: `"type": "coreclr"` + `"request": "launch"` + `"projectPath"`
- **理由**: 
  - `coreclr` type 由 C# DevKit / OmniSharp 提供，支持完整的断点、变量监视、调用栈功能
  - 直接 attach 到进程，而非启动独立命令行窗口
  - `dotnet` type 仅运行 `dotnet run`，调试能力有限
  - 通过 `projectPath` 而非 `program` 启动，避免手动指定输出 DLL 路径

### 2. 引用已有 launchSettings.json 的 profile

- **选择**: 通过 `"launchProfile"` 字段引用 `Properties/launchSettings.json` 中的 `"MinGo.IdServer"` 和 `"MinGo.IdServer.Admin"` profile
- **理由**:
  - DRY 原则：端口、环境变量等配置已经在 `launchSettings.json` 中定义
  - 未来修改端口或环境变量只需改 `launchSettings.json`，`launch.json` 无需同步修改
  - `.NET Core Launch (console)` 模板生成的配置天然支持该字段

### 3. 使用 `preLaunchTask` 确保启动顺序？

- **选择**: 不添加 `preLaunchTask`
- **理由**: 
  - Admin 启动时会轮询 IdServer 的健康检查端点（`AddIdServerEndpointCheck` 已配置）
  - 两个项目启动时间接近，IdServer 通常先就绪
  - 添加 preLaunchTask 会增加配置复杂度和调试启动时间
  - 如果 IdServer 尚未就绪，Admin 会显示连接错误，这是显式的——开发者能立即理解

### 4. launch.json 存放位置

- **选择**: `.vscode/launch.json`（项目根目录下的 `.vscode/` 文件夹）
- **理由**: VS Code 标准规范，该位置自动被 VS Code 识别

### 5. 文件是否提交到 Git

- **选择**: 提交 `.vscode/launch.json` 到代码库
- **理由**: 
  - 该配置是团队共享的开发工具配置，类似 `launchSettings.json`
  - 所有开发者应获得一致的调试体验
  - 已在 `.gitignore` 检查确保不被排除

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 开发者没有安装 C# DevKit / OmniSharp | `coreclr` type 不可用 | VS Code 会提示安装推荐扩展；可在 `.vscode/extensions.json` 添加推荐 |
| compound 中两个进程同时启动，Console 输出交织 | 日志阅读困难 | VS Code DEBUG CONSOLE 会区分输出源（显示进程名） |
| 端口 5001/5002 被占用 | 启动失败 | 与现有 `launchSettings.json` 保持一致，修改端口只需改一处 |
| IdServer 未就绪时 Admin 先启动 | Admin 页面报连接错误 | Admin 已配置健康检查，刷新页面即可 |
