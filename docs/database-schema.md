# 数据库设计

本文档描述 By3 系统当前使用的数据库表结构，对应实体位于 `backend/By3.Repository/Entities/`。

## 实体关系

```
SysUser ||--o{ SysUserRole : 拥有
SysRole ||--o{ SysUserRole : 被分配
SysRole ||--o{ SysRoleMenu : 拥有
SysMenu ||--o{ SysRoleMenu : 被分配
SysDepartment ||--o{ SysUser : 包含
SysPosition ||--o{ SysUser : 包含
SysDictType ||--o{ SysDictData : 包含
SysEmailTemplate ||--o{ SysEmailTemplateVersion : 包含
SysJob ||--o{ SysJobLog : 产生
SysExternalApiToken ||--o{ SysExternalApiAccessLog : 产生
SysExternalApiToken ||--o{ SysExternalApiTokenHistory : 拥有历史
SysExternalApiToken ||--o{ SysExternalApiTokenLog : 拥有操作日志
SysExternalApi ||--o{ SysExternalApiAccessLog : 被访问
```

## 表结构

### SysUser（用户表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| UserName | string | 用户名 |
| PasswordHash | string | BCrypt 密码哈希 |
| Email | string? | 邮箱 |
| Phone | string? | 手机号（存储时加密） |
| RealName | string? | 真实姓名 |
| Gender | string? | 性别（字典值：male/female） |
| DepartmentId | UUID? | 所属部门ID |
| PositionId | UUID? | 职位ID |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysRole（角色表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| RoleName | string | 角色名 |
| Description | string? | 描述 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysMenu（菜单表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| MenuName | string | 菜单名 |
| Permission | string? | 权限标识 |
| Route | string? | 路由 |
| Icon | string? | 图标 |
| Component | string? | 组件路径 |
| MenuType | int | 1=目录 2=菜单 3=按钮 |
| SortOrder | int | 排序 |
| ParentId | UUID? | 父菜单 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysUserRole（用户角色关联表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| UserId | UUID | 用户ID |
| RoleId | UUID | 角色ID |
| CreatedAt | DateTime | 创建时间 |

### SysRoleMenu（角色菜单关联表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| RoleId | UUID | 角色ID |
| MenuId | UUID | 菜单ID |
| CreatedAt | DateTime | 创建时间 |

### SysDepartment（部门表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| DeptName | string | 部门名称 |
| DeptCode | string? | 部门编码 |
| ParentId | UUID? | 父部门ID |
| SortOrder | int | 排序 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysPosition（职位表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| PositionName | string | 职位名称 |
| PositionCode | string? | 职位编码 |
| SortOrder | int | 排序 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysDictType（字典类型表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| DictName | string | 字典名称 |
| DictType | string | 字典类型编码 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysDictData（字典数据表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| DictTypeId | UUID | 字典类型ID |
| DictLabel | string | 显示标签 |
| DictValue | string | 字典值 |
| Remark | string? | 备注 |
| SortOrder | int | 排序 |
| IsDefault | bool | 是否默认值 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysFileRecord（文件记录表）

单文件与多文件上传共用本表，通过 `UploadMode` 区分。

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| FileName | string | 存储文件名 |
| OriginalFileName | string | 原始文件名 |
| StoragePath | string | 存储路径 |
| FileSize | long | 文件大小（字节） |
| ContentType | string | MIME 类型 |
| FileCategory | string | 文件分类 |
| UploadMode | string | single=单文件 / multi=多文件 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysEmailTemplate（邮件模板表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| TemplateCode | string | 模板编码 |
| TemplateName | string | 模板名称 |
| Description | string | 描述 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysEmailTemplateVersion（邮件模板版本表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| TemplateId | UUID | 所属模板ID |
| Version | string | 版本号 |
| Subject | string | 邮件主题 |
| Body | string | 邮件内容 |
| BodyFormat | string | 内容格式：html / text / markdown |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysEmailSetting（邮件发送配置表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| SmtpHost | string | SMTP 服务器地址 |
| SmtpPort | int | SMTP 端口 |
| Username | string | 登录账号 |
| Password | string | 登录密码 |
| FromName | string | 发件人名称 |
| FromAddress | string | 发件人邮箱 |
| EnableSsl | bool | 是否启用 SSL |
| IsEnabled | bool | 是否启用 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |

### SysEmailLog（邮件发送日志表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| TemplateId | UUID? | 模板ID |
| TemplateVersionId | UUID? | 模板版本ID |
| ToAddresses | string | 收件人地址（多个用逗号分隔） |
| CcAddresses | string | 抄送人地址（多个用逗号分隔） |
| Subject | string | 邮件主题 |
| Body | string | 邮件内容 |
| Status | string | 状态：pending / success / failed |
| ErrorMessage | string? | 错误信息 |
| SentAt | DateTime? | 发送时间 |
| CreatedAt | DateTime | 创建时间 |

### SysJob（定时任务表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| JobName | string | 任务名称 |
| JobGroup | string | 任务分组（默认 DEFAULT） |
| JobType | string | 任务类型（与后端 JobTypes 常量对应） |
| CronExpression | string | Cron 表达式 |
| Description | string | 描述 |
| ConfigJson | string | 任务配置 JSON |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysJobLog（定时任务执行日志表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| JobId | UUID | 任务ID |
| JobName | string | 任务名称 |
| FireTime | DateTime | 触发时间 |
| EndTime | DateTime? | 结束时间 |
| Status | string | 状态：Success / Failed |
| Result | string | 执行结果 |
| ExceptionMessage | string? | 异常信息 |
| NextFireTime | DateTime? | 预计下次执行时间 |
| CreatedAt | DateTime | 创建时间 |

### SysExternalApiToken（对外 API Token 表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| AppName | string | 应用名称 |
| ApiKey | string | API Key |
| ApiSecret | string | API Secret（用于签名） |
| Description | string? | 描述 |
| ExpireTime | DateTime? | 过期时间 |
| ExpireType | string | 有效期类型：30/60/90/custom |
| AllowedApiIds | string? | 允许访问的对外接口 ID 列表，JSON 数组；为空表示允许所有 |
| ContactEmail | string | 负责人邮箱，多个用逗号分隔 |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| PreviousValidUntil | DateTime? | 旧 Key 缓冲截止时间，为空则旧 Key 立即失效 |
| PreviousApiKey | string? | 重新生成前的旧 ApiKey，用于缓冲期内继续验证 |
| PreviousApiSecret | string? | 重新生成前的旧 ApiSecret，与 PreviousApiKey 配套 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysExternalApiTokenHistory（对外 API Token 历史凭证表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| TokenId | UUID | 关联的当前 Token Id |
| AppName | string | 应用名称（归档时快照） |
| ApiKey | string | 历史 ApiKey |
| ApiSecret | string | 历史 ApiSecret |
| ExpireTime | DateTime? | 该历史凭证当时的过期时间 |
| ValidUntil | DateTime? | 旧 Key 缓冲截止时间，为空表示立即失效 |
| InvalidatedAt | DateTime? | 手动作废时间，为空表示未作废 |
| InvalidatedBy | UUID? | 手动作废人 Id |
| CreatedAt | DateTime | 创建时间 |

### SysExternalApiTokenLog（对外 API Token 操作日志表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| TokenId | UUID | 关联的 Token Id |
| Action | string | 操作类型：Create/Update/Delete/Regenerate/Enable/Disable |
| ApiKey | string | 操作时的当前 ApiKey |
| IpAddress | string? | 操作人 IP |
| OperatorId | UUID? | 操作人用户 Id |
| OperatorName | string? | 操作人用户名 |
| Remark | string? | 操作备注，例如重生成时的缓冲期说明 |
| CreatedAt | DateTime | 创建时间 |

### SysExternalApiAccessLog（对外 API 访问日志表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| ApiKey | string | 请求使用的 API Key |
| RequestPath | string | 请求路径 |
| RequestMethod | string | 请求方法 |
| RequestParams | string? | 请求参数 |
| IpAddress | string? | IP 地址 |
| Status | string | 状态：success / failed |
| ErrorMessage | string? | 错误信息 |
| IdempotencyKey | string? | 幂等性 Key |
| CreatedAt | DateTime | 创建时间 |

### SysExternalApi（对外 API 接口注册表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| ApiName | string | 接口名称 |
| Route | string | 请求路径，例如 `/external/v1/users` |
| Method | string | 请求方法：GET / POST / PUT / DELETE |
| Description | string? | 接口描述 |
| RateLimitPerSecond | int | 单个 AK 每秒最大请求数，0 表示不限流 |
| RequireIdempotency | bool | 是否要求 `Idempotency-Key` |
| IsEnabled | bool | 是否启用 |
| IsDeleted | bool | 软删除标记 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime? | 更新时间 |
| CreatedBy | UUID? | 创建人 |
| UpdatedBy | UUID? | 更新人 |

### SysAuditLog（操作审计日志表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| UserId | UUID? | 用户ID |
| UserName | string | 用户名 |
| Action | string | 操作描述 |
| Controller | string? | 控制器 |
| RequestPath | string? | 请求路径 |
| RequestMethod | string? | 请求方法 |
| RequestParams | string? | 请求参数（已脱敏） |
| RequestBody | string? | 请求体（已脱敏） |
| RequestHeaders | string? | 请求头 |
| ResponseResult | string? | 响应结果 |
| ResponseHeaders | string? | 响应头 |
| StatusCode | int? | HTTP 状态码 |
| ExceptionMessage | string? | 异常信息 |
| ElapsedMs | long | 耗时毫秒 |
| IpAddress | string? | IP 地址 |
| UserAgent | string? | UA |
| CreatedAt | DateTime | 创建时间 |

### SysLoginLog（登录日志表）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | UUID | 主键 |
| UserId | UUID? | 用户ID |
| UserName | string | 用户名 |
| IsSuccess | bool | 是否成功 |
| Message | string? | 消息 |
| IpAddress | string? | IP 地址 |
| CreatedAt | DateTime | 创建时间 |
