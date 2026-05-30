## 1. 代码修改

- [x] 1.1 在 SidServerSetup.cs 的 IdServerBuilder 链中添加 `.EnableMigration()` 调用
- [x] 1.2 验证文件编译无错误

## 2. 验证

- [x] 2.1 运行 `dotnet build` 确认构建通过
- [x] 2.2 检查 LSP diagnostics 无新增错误
