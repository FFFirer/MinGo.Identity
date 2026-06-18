## Serilog Console 配置

### 需求

- 本地开发时 Serilog 仅输出到 Console
- 使用默认文本格式（无特殊 formatter）

### 当前状态

已满足。base `appsettings.json` 不包含 `WriteTo` 部分，使用 ASP.NET Core 默认 Console 输出。
