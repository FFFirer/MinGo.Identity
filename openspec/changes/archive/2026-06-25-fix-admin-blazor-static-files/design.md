## Context

Admin Blazor 应用的 `Program.cs` 中间件管道缺少 `app.UseStaticFiles()`，导致 Docker Production 模式下所有 `wwwroot/` 静态文件返回 404。

当前管道：
```
ExceptionHandler → UseIdserverAdmin → Antiforgery → MapRazorComponents
```

缺失的 `UseStaticFiles()` 导致 `wwwroot/_framework/blazor.web.js` 等文件无法被服务——这些文件在 `dotnet publish` 阶段已被正确复制到产物中，但运行时没有中间件来处理对它们的 HTTP 请求。

## Goals / Non-Goals

**Goals:**
- 在中间件管道正确位置添加 `UseStaticFiles()`，使 Production 模式下静态文件正常服务
- 确认修复后 `_framework/blazor.web.js` 返回 HTTP 200

**Non-Goals:**
- 不引入 `MapStaticAssets()` 或其他 .NET 8+ 新静态资产方案（保持最小改动）
- 不改动其他服务（IdServer/Migrator）的中间件管道
- 不涉及配置变更或 NuGet 依赖变更

## Decisions

1. **使用 `UseStaticFiles()` 而非 `MapStaticAssets()`**
   - `UseStaticFiles()` 是 ASP.NET Core 标准中间件，成熟稳定，与本项目 .NET 10 兼容
   - `MapStaticAssets()` 是 .NET 8+ 新增的静态资产管道，主要面向 Blazor WebAssembly 和优化场景
   - 本项目 Admin 是 Blazor Server (Interactive Server)，不需要 WASM 的资产指纹/压缩特性
   - 当前发布产物已有 `.br`/`.gz` 预压缩文件，未来如需优化可升级到 `MapStaticAssets()`

2. **中间件放置位置**
   - `UseStaticFiles()` 应放在 `UseExceptionHandler` 之后、`UseIdserverAdmin()` 之前
   - 原因：异常处理应优先于静态文件；静态文件应优先于业务中间件，避免业务中间件无谓处理静态文件请求
   - 推荐顺序：`ExceptionHandler → StaticFiles → UseIdserverAdmin → Antiforgery → MapRazorComponents`

## Risks / Trade-offs

- [低风险] 如果 `UseIdserverAdmin()` 内部已经调用了 `UseStaticFiles()`，额外的调用可能导致两次注册。但 ASP.NET Core 中间件注册是幂等的——同一中间件多次注册只会在管道中多一个环节，不影响正确性。
- [低风险] 无 Breaking Change，纯新增中间件注册。
