# 数据库迁移指南

## 项目结构

```
src/
├── MinGo.Identity.Configuration/     ← 共享配置（StorageConfiguration + ConfigurationHelper）
├── MinGo.Identity.Migrations/        ← 自定义扩展迁移（ExtStoreDbContext + DesignTimeFactory）
├── MinGo.Identity.Migrator/          ← 部署迁移工具（migrate / script 命令）
├── MinGo.IdServer/                   ← 运行时服务（不再自动迁移）
└── MinGo.IdServer.Admin/            ← 管理后台
```

## 设计原则

- **迁移与应用分离**：IdServer 启动时不执行任何迁移，由 Migrator 独立完成
- **配置一致**：Configuration 模块被 IdServer / Migrator / Migrations 三方共用
- **多源配置**：DesignTimeFactory 支持 4 种配置来源，按优先级降序

## 开发时：添加自定义迁移

### 前置条件

确保已安装 `dotnet ef` 工具：

```bash
dotnet tool install --global dotnet-ef
```

### 方式 1（推荐）：UserSecrets

初始化 UserSecrets（仅需一次）：

```bash
cd src/MinGo.Identity.Migrations
dotnet user-secrets init
dotnet user-secrets set "StorageConfiguration:ConnectionString" "Host=localhost;Port=5432;Database=mingo_identity;Username=postgres;Password=123456"
dotnet user-secrets set "StorageConfiguration:Type" "POSTGRE"
```

添加迁移（无需额外参数）：

```bash
cd src/MinGo.Identity.Migrations
dotnet ef migrations add AddUserProfileTable
```

查看待应用迁移：

```bash
dotnet ef migrations list
```

移除最近添加的迁移（未应用前）：

```bash
dotnet ef migrations remove
```

### 方式 2：命令行参数

适合 CI 环境或一次性操作，无需配置 UserSecrets：

```bash
cd src/MinGo.Identity.Migrations
dotnet ef migrations add AddUserProfileTable `
    -- --StorageConfiguration:ConnectionString "Host=localhost;Port=5432;Database=mingo_identity;Username=postgres;Password=123456" `
       --StorageConfiguration:Type "POSTGRE"
```

### 方式 3：项目级 appsettings.json

在 `src/MinGo.Identity.Migrations/` 目录下创建 `appsettings.json`：

```json
{
  "StorageConfiguration": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mingo_identity;Username=postgres;Password=123456",
    "Type": "POSTGRE"
  }
}
```

> 注意：此文件不会自动签入 Git（需手动配置 `.gitignore`），适合团队统一开发配置。

### 配置优先级

```
命令行 args  ←  最高优先级
UserSecrets  ←  本地开发配置
appsettings.json  ←  项目级默认值
内建默认值 (INMEMORY)  ←  最低优先级
```

## 部署时：应用迁移

发布 Migrator：

```bash
dotnet publish src/MinGo.Identity.Migrator -c Release -o publish/migrator
```

### 应用迁移

```bash
dotnet publish/migrator/MinGo.Identity.Migrator.dll migrate
```

### 生成 SQL 脚本（供 DBA 审核）

```bash
dotnet publish/migrator/MinGo.Identity.Migrator.dll script --output up.sql
```

## Docker / K8s 集成

### Init Container 模式（推荐）

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: db-migrate
spec:
  template:
    spec:
      containers:
      - name: migrator
        image: mingo-identity-migrator:latest
        args: ["migrate"]
      restartPolicy: Never
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: idserver
spec:
  template:
    spec:
      containers:
      - name: idserver
        image: mingo-identity-idserver:latest
        # 启动时不再执行迁移
```

### Entrypoint 脚本模式

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
COPY publish/ .

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]
```

```bash
#!/bin/sh
# entrypoint.sh — 先迁移，再启动服务
dotnet MinGo.Identity.Migrator.dll migrate
dotnet MinGo.IdServer.dll
```

## 种子数据

种子数据当前保留在 IdServer 项目（`Conf/Migrations/AfterDeployment/`），后续将迁移到 Migrator 中统一执行。

### 当前状态

| 种子数据 | 执行方式 | 状态 |
|---------|---------|------|
| 默认管理员用户 (administrator) | IdServer 启动时由 IDataSeeder 执行 | ✅ 保留 |
| OIDC 客户端/范围配置 | 同上 | ✅ 保留 |
| 自定义扩展表 | MinGo.Identity.Migrator migrate | ✅ 已实现 |

## 常见问题

### Q: `dotnet ef migrations add` 找不到 appsettings.json？

A: 默认从项目目录（`Directory.GetCurrentDirectory()`）查找。建议使用 UserSecrets 方式配置：

```bash
cd src/MinGo.Identity.Migrations
dotnet user-secrets set "StorageConfiguration:ConnectionString" "..."
```

### Q: 如何切换到 SQL Server？

A: 修改配置的 `Type` 字段即可，Migrator 和 DesignTimeFactory 自动适配：

```json
{
  "StorageConfiguration": {
    "ConnectionString": "Server=...;Database=...;...",
    "Type": "SQLSERVER"
  }
}
```

### Q: 自定义扩展表需要修改已有迁移？

A: 直接修改 `ExtStoreDbContext.cs` 中的实体定义，然后运行：

```bash
dotnet ef migrations add <描述性名称>
```

框架会自动检测模型变化并生成增量迁移。
