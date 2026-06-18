## 设计决策

无需设计变更。纯配置修改，移除 Serilog Console sink 的 formatter 参数。

### 验证

当前 base `appsettings.json` 已不包含 `WriteTo` 部分，使用 ASP.NET Core 默认日志行为（Console 输出）。
