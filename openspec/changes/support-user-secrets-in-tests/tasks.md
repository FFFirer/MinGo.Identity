## 1. 测试项目配置

- [ ] 1.1 在 test.csproj 中添加 `<UserSecretsId>mingo-identity-tests-2026</UserSecretsId>`
- [ ] 1.2 添加 `Microsoft.Extensions.Configuration.UserSecrets` 包引用

## 2. DbFixture 重构

- [ ] 2.1 配置管道增加 `.AddUserSecrets<DbFixture>(optional: true)`
- [ ] 2.2 暴露 `public StorageConfiguration StorageConfiguration { get; }` 属性
- [ ] 2.3 验证配置优先级：env var > UserSecrets > appsettings.json

## 3. CreateMigratorService 复用配置

- [ ] 3.1 `CreateMigratorService()` 改为使用 `_fixture.StorageConfiguration`
- [ ] 3.2 删除 `CreateMigratorService()` 中的独立 ConfigurationBuilder 调用
- [ ] 3.3 验证所有 migrate/script 测试正确使用同一配置源

## 4. 验证

- [ ] 4.1 构建测试项目通过（0 error）
- [ ] 4.2 `dotnet user-secrets init` 可正常初始化
