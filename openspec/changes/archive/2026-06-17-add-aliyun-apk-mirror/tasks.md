## Implementation Tasks

### Task 1: `docker/Dockerfile.idserver` — 添加阿里云 apk 镜像源

将 `RUN apk add` 替换为 `sed` + `apk add` 组合：

```dockerfile
# 替换前
RUN apk add --no-cache icu-libs icu-data-full

# 替换后
RUN sed -i 's|dl-cdn.alpinelinux.org|mirrors.aliyun.com|g' /etc/apk/repositories \
    && apk add --no-cache icu-libs icu-data-full
```

- [x] 确认 sed 替换语法正确
- [x] 确认 `&&` 衔接保证替换失败时 apk 不执行
- [x] 确认镜像层数未增加（合并在同一 RUN）

### Task 2: `docker/Dockerfile.admin` — 添加阿里云 apk 镜像源

与 Task 1 完全相同的改动。

- [x] 应用与 Task 1 相同的 sed + apk add 组合

### Task 3: 验证构建（需在装有 Docker 的机器上执行）

- [ ] 在本地（中国大陆网络）执行 `docker build -f docker/Dockerfile.idserver .` 验证成功
- [ ] 验证容器启动正常，ICU globalization 功能正常

### 非强制但建议

- [ ] （可选）确认境外 CI runner 下构建仍能成功（慢一些但能完成）
