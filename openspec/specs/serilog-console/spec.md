## Serilog Console 配置

### Purpose

定义本地开发和生产环境的日志输出配置规范。

### Requirements

#### Requirement: Console 日志输出
系统 SHALL 将 Serilog 日志输出到 Console，使用默认文本格式。

#### Scenario: 本地开发
- **WHEN** 应用以 Development 环境运行
- **THEN** Serilog 仅输出到 Console，使用默认格式

#### Scenario: 生产环境
- **WHEN** 应用以非 Development 环境运行
- **THEN** Serilog 输出到 Console，使用默认格式
