## Context

### Background
MinGo 正在构建基于 SimpleIdServer 的统一身份验证及授权中心。SimpleIdServer 是一个开源的 .NET 身份服务器，支持 OIDC/OAuth2、SAML 等协议。本项目旨在建立一个可扩展、安全、易于管理的身份基础设施。

### Current State
当前为**绿场项目**（Greenfield），从零开始构建。已确定的技术栈包括：
- .NET 10 (preview)
- SimpleIdServer 6.0.4
- PostgreSQL 16
- Tailwind CSS (用户界面)

### Constraints
- 使用 **Central Package Management (CPM)** 统一管理所有 NuGet 依赖版本
- 必须支持**多租户**（Realms）以满足未来业务扩展需求
- HTTPS 开发证书需跨平台兼容（Windows/macOS/Linux）
- 容器化部署就绪（Docker Compose 本地开发）

## Goals / Non-Goals

**Goals:**
1. 建立双服务架构：IdServer (5001) 作为身份核心，Admin (5002) 作为管理界面
2. 实现完整的 OIDC/OAuth2 端点支持（授权码流、客户端凭证流、Token 验证）
3. 配置 PostgreSQL 持久化存储，支持自动迁移
4. 建立 CPM 机制确保依赖版本一致性
5. 提供可一键启动的本地开发环境

**Non-Goals:**
1. **生产级部署配置**（K8s、负载均衡、高可用）—— 后续迭代实现
2. **完整 UI 定制**（Tailwind 替换 Bootstrap）—— 本次仅建立基础架构，预留扩展点
3. **外部 IdP 集成**（微信、钉钉、GitHub OAuth）—— 架构预留接口，本次不实现
4. **多数据中心/联邦身份** —— 超出本次范围

## Decisions

### 1. 架构模式：SimpleIdServer 原生模板 vs 自建封装
**Decision:** 使用 `idserver` 和 `idserveradmin` 模板作为基础，不额外封装抽象层。

**Rationale:**
- SimpleIdServer 已提供完整的领域模型和工作流引擎，自建封装会增加维护负担
- 模板化启动可快速验证架构可行性
- 后续如需深度定制 UI，可渐进式替换视图层而不影响核心

**Alternatives Considered:**
- **自建封装层**：将 SimpleIdServer 作为内部依赖包装，提供更简化的 API。拒绝原因：过度封装会导致升级困难，SimpleIdServer 的 API 已足够清晰。

### 2. 端口分配：独立端口 vs 子路径路由
**Decision:** 使用独立端口 5001（IdServer）和 5002（Admin），而非子路径（如 /auth 和 /admin）。

**Rationale:**
- 独立端口便于容器化部署和服务发现
- 清晰的服务边界，便于独立扩展（如 Admin 不需要高可用时可单实例）
- 跨域配置更简单（如果未来 Admin 独立部署到不同域名）

**Alternatives Considered:**
- **子路径路由**：/auth 和 /admin 共享端口。拒绝原因：SimpleIdServer 的 Realms 机制已实现子路径隔离，再嵌套一层会增加复杂性；且不利于后续可能的 Admin 独立部署。

### 3. 持久化：PostgreSQL vs SQL Server
**Decision:** 使用 PostgreSQL 而非 SQL Server。

**Rationale:**
- PostgreSQL 在云原生环境和容器化部署中更轻量、生态更完善
- SimpleIdServer 官方支持 PostgreSQL 迁移包
- 符合 MinGo 技术栈一致性（如其他服务已使用 PostgreSQL）

### 4. CPM：启用 Central Package Management
**Decision:** 强制启用 CPM，所有 NuGet 版本在 `Directory.Packages.props` 中集中管理。

**Rationale:**
- SimpleIdServer 生态包含多个相关包（IdServer, IdServer.Pwd, IdServer.Email 等），版本必须严格一致
- EF Core + PostgreSQL 驱动版本匹配至关重要
- 避免未来多服务项目中版本漂移问题

### 5. UI 策略：渐进式替换 vs 全量重写
**Decision:** 采用**渐进式替换策略**：默认使用 SimpleIdServer 内置的 Bootstrap UI，通过 Razor Pages Area 机制逐步覆盖为 Tailwind。

**Rationale:**
- 快速启动：内置 UI 立即可用，无需等待前端开发
- 零停机迁移：可逐个页面替换，而非一次性重写
- 风险可控：如 Tailwind 页面有问题，可回退到默认视图

**Implementation Pattern:**
```
MinGo.IdServer/
├── Areas/
│   └── Password/                    # 覆盖 SimpleIdServer 的默认视图
│       └── Pages/
│           └── Authenticate.cshtml  # Tailwind 定制登录页
```

## Risks / Trade-offs

| Risk | Severity | Mitigation |
|------|----------|------------|
| **.NET 10 Preview 稳定性** | Medium | 项目初期使用 Preview 版本可能遇到 SDK 或 EF Core 兼容性问题 | 锁定特定 Preview 版本，建立 CI/CD 快速验证构建；关注 .NET 10 RC 发布时间表，及时迁移 |
| **SimpleIdServer 版本升级** | Low-Medium | 6.0.4 → 7.x 可能引入 Breaking Changes | 使用 CPM 集中管理版本，升级前在本地环境充分测试；关注官方迁移指南 |
| **Realms 多租户复杂度** | Medium | SimpleIdServer 的 Realms 功能强大但学习曲线陡峭 | 初期只使用默认 `master` Realm，后续业务扩展时再启用多租户；文档化 Realms 使用规范 |
| **HTTPS 开发证书跨平台** | Low | dotnet dev-certs 在 Linux/WSL 下配置较复杂 | 提供详细的跨平台证书配置文档；考虑使用反向代理（如 Caddy）统一 TLS 终止 |
| **Tailwind CDN 生产性能** | Low | CDN 引入方式在弱网环境下加载较慢 | 开发阶段使用 CDN 快速迭代；生产构建时考虑引入 Tailwind CLI 生成优化后的 CSS |

## Migration Plan

### 本地开发环境启动步骤
1. **数据库启动**：`docker-compose up -d postgres`
2. **还原依赖**：`dotnet restore`（自动识别 CPM 配置）
3. **启动 IdServer**：`cd src/MinGo.IdServer && dotnet run --urls=https://localhost:5001`
4. **启动 Admin**：`cd src/MinGo.IdServer.Admin && dotnet run --urls=https://localhost:5002`
5. **验证**：访问 `https://localhost:5001/.well-known/openid-configuration`

### 验证清单
- [ ] OIDC Discovery 文档返回正确 JSON
- [ ] 使用 administrator/password 登录 Admin 界面
- [ ] 创建测试客户端并验证 Authorization Code 流程
- [ ] 验证 Token Introspection 端点返回预期结果

### 回滚策略
- 数据库：保留 Docker Volume，删除容器即可重置
- 证书：`dotnet dev-certs https --clean` 清除后重新生成
- 代码：所有变更提交 Git，可随时回退

## Open Questions

1. **Realms 命名规范**：未来多租户时，Realm 命名采用 `tenant-{id}` 还是业务语义名称（如 `prod`、`staging`）？
2. **会话存储**：是否需要引入 Redis 存储会话状态，还是依赖 PostgreSQL？取决于并发量需求。
3. **邮件服务**：SimpleIdServer 的邮件通知默认支持控制台和 SMTP，生产环境需要配置实际邮件服务商。
4. **监控集成**：是否需要集成 OpenTelemetry/Health Checks？建议后续迭代添加。
