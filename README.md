# MinGo.Identity

统一身份验证及授权中心 - 基于 SimpleIdServer 构建的集中式身份管理解决方案。

## 🏗️ 架构概览

```
┌─────────────────────────────────────────────────────────────────┐
│                    MinGo.Identity 架构                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐         ┌─────────────┐                        │
│  │  IdServer   │         │   Admin     │                        │
│  │  :5001      │◄────────│  :5002      │                        │
│  │             │  OIDC   │             │                        │
│  │ • OIDC/OAuth│         │ • 管理后台  │                        │
│  │ • Token颁发 │         │ • 用户管理  │                        │
│  │ • PostgreSQL│         │ • 客户端配置│                        │
│  └──────┬──────┘         └─────────────┘                        │
│         │                                                       │
│         └──────────▶ PostgreSQL (Port 5432)                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## 🚀 快速开始

### 前置要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- [Docker](https://www.docker.com/products/docker-desktop) (用于 PostgreSQL)
- PowerShell 或 Bash

### 1. 启动数据库

```powershell
# 启动 PostgreSQL 容器
docker-compose up -d postgres

# 验证数据库状态
docker-compose ps
```

### 2. 还原依赖

```powershell
# 还原所有 NuGet 包（自动使用 CPM）
dotnet restore
```

### 3. 启动服务

**启动 IdServer (端口 5001):**

```powershell
cd src/MinGo.IdServer
dotnet run
```

验证：`https://localhost:5001/.well-known/openid-configuration`

**启动 Admin (端口 5002):**

```powershell
cd src/MinGo.IdServer.Admin
dotnet run
```

访问：`https://localhost:5002/master/clients`

### 4. 默认账号

- **用户名**: `administrator`
- **密码**: `password`

## 📁 项目结构

```
MinGo.Identity/
├── Directory.Packages.props       # CPM - 集中包版本管理
├── Directory.Build.props          # 全局 MSBuild 属性
├── nuget.config                   # NuGet 源配置
├── docker-compose.yml             # 本地开发环境
├── src/
│   ├── MinGo.IdServer/            # 身份服务核心 (Port 5001)
│   │   ├── Program.cs             # SimpleIdServer 配置
│   │   ├── appsettings.json       # 生产配置
│   │   └── appsettings.Development.json
│   └── MinGo.IdServer.Admin/      # 管理后台 (Port 5002)
│       ├── Program.cs             # Admin 服务配置
│       └── appsettings.json
├── openspec/                      # 架构规范文档
└── docs/                          # 详细文档
```

## 🔧 技术栈

| 组件 | 版本 | 说明 |
|------|------|------|
| .NET | 10.0 (Preview) | 运行时和 SDK |
| SimpleIdServer | 6.0.4 | 身份服务器框架 |
| PostgreSQL | 16 | 持久化存储 |
| EF Core | 9.0.4 | ORM |
| Tailwind CSS | CDN | 用户界面 (未来扩展) |

## 📋 Central Package Management

本项目使用 **CPM** 集中管理 NuGet 包版本。所有版本定义在 `Directory.Packages.props`：

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="SimpleIdServer.IdServer" Version="6.0.4" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
```

项目文件只需引用包名（无需版本号）：

```xml
<!-- src/MinGo.IdServer/MinGo.IdServer.csproj -->
<PackageReference Include="SimpleIdServer.IdServer" />
```

### 添加新包

1. 在 `Directory.Packages.props` 中添加版本定义
2. 在项目文件中引用（无版本号）
3. 运行 `dotnet restore`

### 升级包版本

只需修改 `Directory.Packages.props` 中的版本号，所有项目自动使用新版本。

## 🌐 端口分配

| 服务 | 端口 | 说明 |
|------|------|------|
| IdServer | 5001 | 身份服务核心，OIDC/OAuth2 端点 |
| Admin | 5002 | 管理后台，连接 IdServer |
| PostgreSQL | 5432 | 数据库服务 |

## 🔐 支持的协议

- **OpenID Connect (OIDC)**: 授权码流、隐式流、混合流
- **OAuth 2.0**: 授权码流、客户端凭证流、刷新令牌
- **SAML 2.0**: 企业集成 (预留)
- **WS-Federation**: 企业集成 (预留)

## 🧪 开发指南

### 验证 OIDC Discovery

```bash
curl https://localhost:5001/.well-known/openid-configuration
```

### 测试 Token 端点

```bash
# 客户端凭证流
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=test_client" \
  -d "client_secret=test_secret" \
  -d "scope=api1"
```

## 🛠️ 故障排除

### HTTPS 证书错误

```powershell
# 清除并重新生成开发证书
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 数据库连接失败

```powershell
# 检查 PostgreSQL 容器状态
docker-compose ps
docker-compose logs postgres

# 重置数据库（数据会丢失）
docker-compose down -v
docker-compose up -d postgres
```

### 包还原失败

```powershell
# 清除 NuGet 缓存
dotnet nuget locals all --clear

# 重新还原
dotnet restore --force
```

## 📝 后续规划

- [ ] 完整 UI 定制 (Tailwind CSS 替换 Bootstrap)
- [ ] 外部 IdP 集成 (微信、钉钉、GitHub)
- [ ] 多租户 (Realms) 支持
- [ ] 生产环境部署配置 (K8s)
- [ ] 监控和日志集成

## 📄 许可证

Copyright © 2026 MinGo. All rights reserved.

## 🔗 参考

- [SimpleIdServer Documentation](https://simpleidserver.com/docs/installation/template)
- [OpenID Connect Specification](https://openid.net/specs/openid-connect-core-1_0.html)
- [.NET 10 Preview](https://dotnet.microsoft.com/download/dotnet/10.0)
