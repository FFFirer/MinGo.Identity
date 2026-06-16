## Context

MinGo.Identity 包含三个可执行项目：`MinGo.IdServer` (OIDC/OAuth 服务)、`MinGo.IdServer.Admin` (Blazor Server 管理后台)、`MinGo.Identity.Migrator` (CLI 迁移工具)。前两者运行在 ASP.NET Core 上，Migrator 为纯控制台应用。

当前日志通过 ASP.NET Core 默认的 `ConsoleLoggerProvider` 输出到 stdout，格式为纯文本，级别控制仅靠 `Logging:LogLevel`。生产环境 Docker 部署需要结构化 JSON 日志以对接容器编排系统的日志采集（如 EFK / Loki），开发环境则希望可读文本。

### 现有相关基础设施
- **OpenTelemetry**: 已在 Admin 和 IdServer 中配置 Tracing（Console + OTLP 导出器），但与日志管线独立
- **CPM** (Central Package Management): 通过 `Directory.Packages.props` 统一管理包版本
- **配置分层**: 各项目通过 `appsettings.json` + `appsettings.{env}.json` + 环境变量 3 层配置合并

### 约束
- `MinGo.Identity.Migrator` 不改动，保持 `Console.WriteLine`
- 不引入 OTel Logging SDK，日志与 Trace 保持独立管线
- 不改动现有 SimpleIdServer 的内部日志调用

## Goals / Non-Goals

**Goals:**
- Serilog 接管 Admin 和 IdServer 两个 ASP.NET Core 项目的日志管线
- 生产/Docker 环境输出结构化 JSON 到 stdout
- 开发环境输出可读纯文本
- 通过配置文件（而非代码）区分环境日志格式
- 最简增强：仅引入 `Serilog.AspNetCore`，不添加额外 enricher

**Non-Goals:**
- 不接入 File / Seq / Elasticsearch 等额外 sink
- 不改造 Migrator 项目
- 不引入 OTel Logging SDK (OpenTelemetry 与 Serilog 保持独立)
- 不修改现有 ILogger<T> 调用点
- 不修改 SimpleIdServer 内部的日志行为

## Decisions

### 1. 包选型: Serilog.AspNetCore

| Option | 评估 |
|--------|------|
| **Serilog.AspNetCore** (选) | 一站式包，自动拉取 Serilog、Sinks.Console、Sinks.File、Settings.Configuration。一行 `UseSerilog()` 即完成集成 |
| 手动组合各个包 | 更细粒度但增加维护负担，无实质收益 |

Decision: 使用 `Serilog.AspNetCore`，额外加 `Serilog.Formatting.Compact` 以获得 CompactJsonFormatter。

### 2. 接入方式: ReadFrom.Configuration

代码统一使用 `ReadFrom.Configuration()` 纯配置驱动，不包含环境判断逻辑。

```csharp
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});
```

环境间差异化通过 ASP.NET Core 配置分层实现：
- `appsettings.json` → Console + CompactJsonFormatter (Docker/生产)
- `appsettings.Development.json` → Console 无 formatter (可读文本)

`WriteTo` 数组在 ASP.NET Core 配置合并时按 **索引替换**，因此 Development 文件只需声明 `WriteTo: [{Name: "Console"}]` 即可覆盖 base 的 JSON 配置。

### 3. 日志级别控制

双重级别过滤的叠加关系：

```
ILogger<T>.Log(...)
  → ASP.NET Core Logging:LogLevel 过滤 (保留不动)
    → Serilog MinimumLevel 过滤 (主控)
      → Serilog WriteTo Console 输出
```

建议将主控权放在 `Serilog:MinimumLevel` 节中，`Logging:LogLevel` 保留作为后备。由于 Serilog 在 `UseSerilog()` 接管后会优先使用自己的级别判断，`Logging` 节的实际影响最小。

### 4. Register 时机

`UseSerilog()` 必须在 SimpleIdServer 的 Add 方法之前调用，以确保 SimpleIdServer 内部通过 ILoggerFactory 创建的日志也走 Serilog 管线。

```
Admin Program.cs:
  var builder = ...;
  builder.Host.UseSerilog(...)          // ← 最先
  builder.Configuration.AddJsonFile...
  builder.AddIdserverAdmin(...)         // ← SimpleIdServer 初始化在后

IdServer Program.cs:
  var builder = ...;
  builder.Host.UseSerilog(...)          // ← 最先
  builder.Configuration.AddJsonFile...
  SidServerSetup.ConfigureIdServer(...) // ← SimpleIdServer 初始化在后
```

### 5. Enrichment 策略

仅 `FromLogContext` —— 这是 Serilog.AspNetCore 默认行为，零代价。不添加 MachineName/ThreadId/Environment 等额外 enricher。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| Serilog 版本与 .NET 10 Preview 不兼容 | `Serilog.AspNetCore` 长期保持与最新 .NET 的兼容性，实测验证即可 |
| `appsettings.Development.json` 的 WriteTo 数组覆盖可能因配置合并行为异常 | 验证方式: 分别在 Development 和 Production 环境启动，确认日志格式差异正确 |
| 双重级别过滤 (Logging + Serilog) 可能造成调试困扰 | 在 appsettings 注释说明两者关系；文档记录 |
| IdServer 的 `SidServerSetup` 内部暗含日志配置 | 阅读 SimpleIdServer 源码确认无硬编码日志初始化。如有，需在 `UseSerilog()` 之后补 `WithFilter()` 矫正 |
