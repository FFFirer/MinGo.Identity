## 1. 包版本降级

- [ ] 1.1 Directory.Packages.props: 降级 Microsoft.EntityFrameworkCore 及其衍生包到 8.0.28
- [ ] 1.2 Directory.Packages.props: 降级 Npgsql.EntityFrameworkCore.PostgreSQL 到 8.0.10
- [ ] 1.3 Directory.Packages.props: 降级 Pomelo.EntityFrameworkCore.MySql 到 8.0.2

## 2. 清理 NoWarn

- [ ] 2.1 MinGo.Identity.Migrator.csproj: 移除 NU1608 的 `<NoWarn>`

## 3. 构建验证

- [ ] 3.1 dotnet restore 确认无版本冲突
- [ ] 3.2 dotnet build 确认 NU1608 消除、编译通过
- [ ] 3.3 lsp_diagnostics 确认零错误
