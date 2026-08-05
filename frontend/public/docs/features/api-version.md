# API 版本控制

## 功能说明

基于 URL 路径段实现 API 版本控制，便于接口平滑升级，支持多版本共存。

## 版本规则

| 规则 | 说明 | 示例 |
|------|------|------|
| 路径格式 | `/api/v{版本}/资源` | `/api/v1/users` |
| 默认版本 | 未指定时自动使用 v1 | `/api/users` → v1 |
| 版本发现 | 响应头包含支持的版本信息 | `api-supported-versions: 1.0` |

## Swagger 文档

| 环境 | 访问地址 | 说明 |
|------|---------|------|
| 开发环境 | `http://localhost:5000/swagger` | 默认启用 |
| 生产环境 | 需设置 `Swagger__IsEnabled=true` | 默认关闭 |

功能：按版本分组展示接口、在线测试、自动携带 JWT 认证、显示 XML 注释。

## 配置项

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| `Swagger__IsEnabled` | Development 启用 | 是否启用 Swagger UI |

## 扩展指南

新增版本时：Controller 标注 `[ApiVersion("2.0")]`，路由使用 `api/v{version:apiVersion}/[controller]`，旧版本保持兼容。
