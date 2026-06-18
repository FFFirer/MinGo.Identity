## Why

基础配置 (`appsettings.json`) 使用 `RenderedCompactJsonFormatter` 输出结构化 JSON 日志，但该 formatter 需要 `Serilog.Formatting.Compact` 包。Development 配置已通过覆盖使用纯文本 Console 输出。为简化配置并消除不必要的包依赖，移除 base 配置中的 formatter。

## What Changes

- 移除 `MinGo.IdServer/appsettings.json` 中 Serilog Console sink 的 `formatter` 参数
- 移除 `MinGo.IdServer.Admin/appsettings.json` 中 Serilog Console sink 的 `formatter` 参数

## Capabilities

### New Capabilities

无

### Modified Capabilities

无

## Impact

- 两个项目的日志格式从 JSON 变为默认文本格式
- `Serilog.Formatting.Compact` 包不再被运行时需要
