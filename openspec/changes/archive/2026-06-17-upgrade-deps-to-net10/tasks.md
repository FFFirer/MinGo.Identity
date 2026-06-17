## 1. Microsoft 生态 → 10.0.9

- [x] 1.1 升级 EF Core 系列：EntityFrameworkCore / Design / SqlServer / InMemory / Relational / Sqlite → 10.0.9
- [x] 1.2 升级 Microsoft.Extensions.*：Configuration / Binder / Json / EnvironmentVariables / UserSecrets / CommandLine → 10.0.9
- [x] 1.3 升级 ASP.NET Core 系列：Auth (Facebook/Google/Negotiate) / Mvc.Razor.RuntimeCompilation → 10.0.9
- [x] 1.4 升级缓存系列：Caching.SqlServer / Caching.StackExchangeRedis → 10.0.9
- [x] 1.5 升级 Microsoft.NET.Test.Sdk 17.13.0 → 18.6.0

## 2. 数据库 Provider

- [x] 2.1 升级 Npgsql 9.0.3 → 10.0.3
- [x] 2.2 升级 Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4 → 10.0.2

## 3. 第三方独立包

- [x] 3.1 升级 Serilog.AspNetCore 9.0.0 → 10.0.0
- [x] 3.2 升级 OpenTelemetry 系列：Exporter.Console / OTLP / Api → 1.16.0
- [x] 3.3 升级 Hangfire.PostgreSql 1.20.10 → 1.21.1 及 Hangfire.SqlServer 1.8.18 → 1.8.23
- [x] 3.4 升级缓存包：Community.PgSql 4.0.6 → 6.0.2 / NeoSmart.Sqlite 8.0.0 → 9.0.1 / Pomelo.MySql 2.2.0 → 2.2.1

## 4. 安全修复 pin

- [x] 4.1 升级 Microsoft.Bcl.Memory 10.0.4 → 10.0.9 及 System.Security.Cryptography.Xml 10.0.6 → 10.0.9

## 5. 测试工具

- [x] 5.1 升级 xunit.runner.visualstudio 2.8.2 → 3.1.5
- [x] 5.2 升级 coverlet.collector 6.0.4 → 8.0.1

## 6. 构建验证

- [x] 6.1 执行 dotnet restore 确认依赖解析无冲突
- [x] 6.2 执行 dotnet build 确认全部项目编译通过
- [x] 6.3 执行 dotnet test 确认全部测试通过
