export const APP_NAME = 'By3'
export const APP_VERSION = 'v1.0.0'
export const APP_COPYRIGHT = '© 2026 By3 Team. All rights reserved.'
export const APP_DESCRIPTION = '企业级后台管理框架，集成权限控制、系统监控、字典管理、文件管理等能力。'

export const APP_FEATURES = [
  { code: 'rbac', module: '权限控制', description: '基于 RBAC 模型的用户、角色、菜单权限管理，支持按钮级权限控制', tags: ['RBAC', 'JWT', '菜单权限', '按钮权限'] },
  { code: 'auth', module: '身份认证', description: 'JWT Token 认证机制，支持 Token 刷新与黑名单失效', tags: ['JWT', '登录认证', 'Token 刷新'] },
  { code: 'idempotency', module: '接口幂等', description: '通过 Idempotency-Key 请求头防止接口重复提交', tags: ['幂等', '防重放'] },
  { code: 'audit', module: '请求审计', description: '自动记录请求参数、响应结果、执行耗时、IP 等操作日志', tags: ['审计日志', '操作追踪'] },
  { code: 'login-log', module: '登录日志', description: '记录用户登录时间、IP、是否成功等登录行为', tags: ['登录日志', '安全审计'] },
  { code: 'rate-limit', module: '限流保护', description: '固定窗口限流，针对登录接口单独配置防暴力破解', tags: ['限流', '防爆破'] },
  { code: 'compression', module: '响应压缩', description: 'HTTP 响应压缩，提升接口传输效率', tags: ['Gzip', '性能优化'] },
  { code: 'organization', module: '组织机构', description: '部门树形管理与岗位管理，支持人员信息关联', tags: ['部门', '岗位'] },
  { code: 'dict', module: '字典管理', description: '系统字典类型与字典数据管理，启动自动加载并支持前端缓存刷新', tags: ['字典', '缓存'] },
  { code: 'file', module: '文件管理', description: '单文件/多文件上传，支持拖拽上传与按扩展名分类校验', tags: ['文件上传', '拖拽上传', '扩展名校验'] },
  { code: 'email', module: '邮件管理', description: '邮件模板与版本管理，支持邮件发送与发送日志', tags: ['邮件模板', '邮件发送'] },
  { code: 'job', module: '任务调度', description: '定时任务与任务执行日志管理', tags: ['定时任务', 'Job'] },
  { code: 'theme', module: '主题设置', description: '支持主题色切换、暗黑模式、侧边栏风格等个性化设置', tags: ['主题', '暗黑模式'] },
  { code: 'api-version', module: 'API 版本控制', description: '基于 URL 路径的 API 版本控制与 Swagger 文档', tags: ['API 版本', 'Swagger'] },
  { code: 'external-api', module: '对外 API', description: 'ApiKey + HMAC-SHA256 签名认证，提供可复用的 C#/JS 调用示例', tags: ['OpenAPI', 'HMAC-SHA256', '签名认证'] }
]

export const CHANGELOG = [
  { version: 'v1.0.0', date: '2026-08-01', items: ['系统初始化发布', '完成用户、角色、菜单权限管理', '新增部门、岗位、字典、文件、邮件、任务调度模块', '首页新增系统版本、功能说明与更新日志'] }
]
