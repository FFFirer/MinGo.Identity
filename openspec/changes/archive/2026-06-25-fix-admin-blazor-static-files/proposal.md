## Why

Admin Blazor 应用在 Docker 容器中运行时，所有静态资源（`_framework/blazor.web.js`、`_content/*`、`lib/*` 等）返回 404。页面 HTML 能正常渲染（由 Razor Components 端点提供），但 Blazor Server 所需的核心 JS 文件无法加载，导致交互功能完全失效。

根本原因：`Program.cs` 中间件管道缺少 `app.UseStaticFiles()`，在 Production 模式下，发布到 `wwwroot/` 的静态文件不被服务。本地 Development 模式因 .NET SDK 自动注入 Static Web Assets 中间件而掩盖了此问题。

## What Changes

- `src/MinGo.IdServer.Admin/Program.cs`: 在中间件管道中添加 `app.UseStaticFiles()`，置于 `UseIdserverAdmin()` 之前
- 不涉及 NuGet 依赖变更、不涉及配置变更、不涉及新的 capability

## Capabilities

### New Capabilities

无新增 capability。本次变更是对已有 `idserver-admin` capability 的缺陷修复。

### Modified Capabilities

无 spec 级别变更。`UseStaticFiles()` 是 ASP.NET Core 标准中间件，不影响现有功能合约。

## Impact

- 仅修改 `src/MinGo.IdServer.Admin/Program.cs`，在 `UseExceptionHandler` 和 `UseIdserverAdmin` 之间插入一行 `app.UseStaticFiles()`
- 无 API/依赖变更，无 Breaking Change
- 修复后，所有 `wwwroot/` 下的静态文件（`_framework/blazor.web.js`、`_content/*`、`lib/*`）将得到正确服务
