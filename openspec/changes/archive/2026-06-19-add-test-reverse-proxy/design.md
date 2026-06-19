## Context

当前集成测试环境使用 `localhost:5001` 和 `localhost:5002` 访问服务，无法验证生产环境的反向代理配置。生产环境通常使用 Nginx/Traefik 作为反向代理，通过域名和标准端口访问服务。

## Goals / Non-Goals

**Goals:**
- 添加 Nginx 反向代理配置
- 配置自定义域名解析（`idserver.local`、`admin.local`）
- 支持 HTTP (8080) 和 HTTPS (8443) 访问
- 模拟生产环境的访问方式

**Non-Goals:**
- 不修改生产环境配置
- 不影响本地开发环境
- 不引入新的测试框架

## Decisions

### 1. 反向代理选择

**选择**: Nginx

**理由**:
- 轻量级，配置简单
- 生产环境常用，配置可复用
- Docker 镜像小，启动快

**替代方案**:
- Traefik → 配置更复杂，适合动态环境
- Caddy → 自动 HTTPS，但学习成本高

### 2. 域名解析方案

**选择**: 使用 Docker 网络 + 自定义 DNS

**配置**:
```yaml
networks:
  mingo-network:
    driver: bridge
    driver_opts:
      com.docker.network.bridge.host_mapping_ipv4: "127.0.0.1"
```

在 docker-compose.test.yml 中配置 `extra_hosts`:
```yaml
nginx:
  extra_hosts:
    - "idserver.local:127.0.0.1"
    - "admin.local:127.0.0.1"
```

**替代方案**:
- 修改 hosts 文件 → 需要管理员权限，不自动化
- 使用 dnsmasq → 增加复杂度

### 3. HTTPS 配置

**选择**: 使用自签名证书

**配置**:
- 生成自签名证书用于测试
- Nginx 配置 SSL 终止
- 客户端配置忽略证书错误

**替代方案**:
- Let's Encrypt → 需要公网域名
- 仅 HTTP → 无法测试 HTTPS 配置

### 4. 端口映射

**选择**:
- HTTP: 8080 → Nginx → 服务
- HTTPS: 8443 → Nginx → 服务

**理由**:
- 避免与本地 80/443 端口冲突
- 模拟生产环境的端口配置
- 便于测试和调试

## Risks / Trade-offs

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|----------|
| 自签名证书不被信任 | 高 | 低 | 配置客户端忽略证书错误 |
| 域名解析失败 | 中 | 中 | 使用 Docker 网络 + extra_hosts |
| Nginx 配置错误 | 中 | 中 | 提供配置模板和验证脚本 |
| 端口冲突 | 低 | 低 | 使用非标准端口 |

## Migration Plan

1. **创建 Nginx 配置文件**
2. **生成自签名证书**（可选）
3. **修改 docker-compose.test.yml**
4. **更新环境变量**
5. **测试验证**

## Open Questions

1. 是否需要支持 HTTPS？还是仅 HTTP (8080)？
2. 域名是 `idserver.local` 还是其他？
3. 是否需要提供配置脚本自动化生成证书？
