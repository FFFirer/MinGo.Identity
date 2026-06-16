## Why

项目目前所有服务（IdServer、Admin、Migrator）均使用 ASP.NET Core 默认的 `ConsoleLoggerProvider`，日志输出为纯文本、非结构化，缺乏按照环境切换格式、级别精细过滤、以及后续接入集中式日志系统的基础能力。在容器化部署（Docker）场景下，统一的结构化日志输出是运维可观测性的前提。

## What Changes

- **Directory.Packages.props**: 新增 `Serilog.AspNetCore` 和 `Serilog.Formatting.Compact` 包版本
- **MinGo.IdServer**: Program.cs 添加 `UseSerilog()`；appsettings.json 添加 Serilog 配置节；新建 appsettings.Development.json 覆盖开发环境日志格式
- **MinGo.IdServer.Admin**: Program.cs 添加 `UseSerilog()`；appsettings.json 添加 Serilog 配置节；新建 appsettings.Development.json 覆盖开发环境日志格式
- **MinGo.Identity.Migrator**: 不改动，保持 `Console.WriteLine`

## Capabilities

### New Capabilities
- `structured-logging`: 统一的结构化日志管线，通过 Serilog 接管所有 ASP.NET Core 服务的日志输出

### Modified Capabilities

无。现有 specs 不涉及日志行为层面的需求变更。

## Impact

| 维度 | 影响 |
|------|------|
| **代码** | Admin + IdServer 各 Program.cs 各加 ~3 行；两个项目 csproj 各加一条 PackageReference |
| **配置** | 两个项目 appsettings.json 新增 `Serilog` 节；各新建 appsettings.Development.json |
| **包管理** | Directory.Packages.props 新增 2 个包版本 |
| **日志行为** | Docker/生产环境: JSON 结构化输出到 stdout；开发环境: 可读纯文本 |
| **不变** | Migrator 项目不受影响；现有 Logging 节保留；所有 ILogger<T> 调用不变 |
