## Why

MinGo 生态系统需要统一的身份验证及授权中心，为所有业务系统（Web、移动端、API 服务）提供集中化的身份管理能力。当前各系统独立处理认证，导致用户体验不一致、安全策略难以统一管理。建立基于 SimpleIdServer 的专业身份中心，采用 .NET 10 + PostgreSQL 技术栈，能够支撑多协议（OIDC/OAuth2/SAML）、多租户、可扩展的认证需求。

## What Changes

- **初始化 MinGo.Identity 解决方案结构**，建立 Central Package Management (CPM) 机制统一管理 NuGet 依赖版本
- **部署 SimpleIdServer IdServer** (端口 5001)，包含完整的 OIDC/OAuth2 端点、用户认证流程、PostgreSQL 持久化
- **部署 SimpleIdServer Admin** (端口 5002)，提供基于 Radzen 的管理后台用于运维配置
- **建立 PostgreSQL 数据库架构**，存储用户、客户端、范围、密钥等身份数据
- **定制用户界面**（后续迭代），使用 Razor Pages + Tailwind CSS 替换默认 Bootstrap 登录/注册页面
- **配置 HTTPS 开发证书和本地 Docker Compose 环境**，支持一键启动开发环境

## Capabilities

### New Capabilities
- `identity-server-core`: SimpleIdServer 核心身份服务配置，包括 OIDC/OAuth2 端点、Token 颁发与验证、PostgreSQL 持久化
- `identity-admin-portal`: 基于 SimpleIdServer.Admin 的管理后台，用于客户端管理、用户管理、配置中心
- `central-package-management`: NuGet 包中央管理机制，统一控制 SimpleIdServer 生态及 EF Core、PostgreSQL 驱动版本
- `identity-ui-customization`: 用户界面定制能力，支持 Razor Pages 视图覆盖和 Tailwind CSS 样式替换
- `devops-local-environment`: 本地开发环境配置，包括 Docker Compose 编排 PostgreSQL、HTTPS 证书配置

### Modified Capabilities
- 无现有规范修改

## Impact

- **新增基础设施依赖**: PostgreSQL 16+ 数据库服务，需要规划生产环境高可用部署
- **端口预留**: 开发环境预留端口 5001 (IdServer) 和 5002 (Admin)，需确保无冲突
- **NuGet 依赖**: 引入 SimpleIdServer 生态包 (6.0.4) 及 .NET 10 预览版依赖
- **后续扩展**: 此架构为未来独立用户门户 (Razor Pages + Tailwind) 留出演进空间
