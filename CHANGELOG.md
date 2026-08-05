# Changelog

本项目所有重要变更都记录在此文件。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### Added
- 密码策略：密码须包含大写字母、小写字母和数字，最少8位
- 登录失败锁定：5分钟内连续失败5次则锁定15分钟
- 首次登录强制修改密码：`LoginResultDto.NeedChangePassword` 标记
- 用户自主修改密码接口：`POST /api/v1/auth/change-password`
- 健康检查端点：`GET /api/health`，检查数据库连接状态
- 测试覆盖率报告脚本：`scripts/coverage-report.bat` / `.sh`
- 性能压测脚本：`scripts/loadtest/k6-loadtest.js`
- 代码规范：StyleCop.Analyzers 集成
- 静态代码分析：`.editorconfig` C# 规则扩展
- API 文档：Service/Repository 层 XML 注释支持
- HTTPS 强制跳转：生产环境启用 HSTS
- 架构文档：`docs/architecture.md`（系统架构图、ER图、流程图）
- CD 工作流：`.github/workflows/release.yml`（tag 触发自动发布）
- 第三方声明：`THIRD-PARTY-NOTICES.txt`
- AGENTS.md：项目开发指引
- CONTRIBUTING.md：贡献指南
- SECURITY.md：安全策略
- LICENSE：Apache 2.0 许可证

### Changed
- `DbSeeder` 默认管理员密码从配置 `Jobs:UserSeed:DefaultPassword` 读取，不再硬编码
- `CreateUserValidator` 密码最低长度从6位提升至8位
- `SysUser` 新增 `PasswordChangedAt` 字段
- Swagger 配置加载 Service/Repository 的 XML 注释文件

### Security
- 移除 DbSeeder 中硬编码的默认密码
- 登录接口添加暴力破解防护（失败锁定机制）

## [1.0.0] - 2026-08-04

### Added
- 用户管理（CRUD、角色分配、密码重置）
- 角色管理（CRUD、菜单权限分配）
- 菜单管理（目录/菜单/按钮三级树形结构）
- 部门管理（树形组织机构）
- 岗位管理
- 字典管理（类型与数据）
- 文件管理（单文件/多文件上传）
- 邮件管理（模板、版本、发送日志）
- 系统设置（邮件发送端配置）
- 任务调度（Quartz.NET 定时任务）
- 对外 API（AK/SK 签名认证、幂等/限流）
- JWT 认证 + Permission 授权
- Token 刷新、登出、黑名单
- 动态路由与按钮级权限
- 操作日志、登录日志
- 接口幂等性（Idempotency-Key）
- 内存缓存
- 软删除与审计字段
- 手机号加密存储
- 审计日志敏感信息脱敏
- Docker 部署支持
- GitHub Actions CI（构建、测试、安全扫描）
