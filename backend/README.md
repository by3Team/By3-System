# By3 后端

基于 ASP.NET Core 10 的 By3 权限管理后端。

## 技术栈

| 组件 | 选型 | 说明 |
|------|------|------|
| 后端框架 | ASP.NET Core 10 | 原生 Web API |
| ORM | Entity Framework Core 9 | 配合 Npgsql 访问 PostgreSQL |
| 数据库 | PostgreSQL 16 | 关系型数据库 |
| 缓存 | IMemoryCache | 进程内缓存，无外部依赖 |
| 认证 | JWT | 无状态 Token 认证 |
| 参数验证 | FluentValidation | 声明式 DTO 校验 |
| 幂等性 | 自定义 IdempotencyFilter | 基于 `Idempotency-Key` 请求头 |
| 任务调度 | Quartz.NET | 定时任务与持久化配置 |
| 邮件 | MailKit | SMTP 邮件发送与模板管理 |
| 日志 | ILogger + 数据库 | 操作日志、登录日志异步落库 |
| 依赖注入 | 原生 DI | 无第三方容器 |

## 项目结构

```
backend/
├── By3.Api/                    # Web 主机
│   ├── Authorization/          # 权限处理器
│   ├── Controllers/            # API 控制器
│   ├── Filters/                # 全局过滤器（异常、审计、幂等性）
│   ├── Middleware/             # 自定义中间件
│   ├── Program.cs              # 启动配置
│   ├── Properties/             # 启动配置
│   └── appsettings*.json       # 应用配置
├── By3.Service/                # 业务逻辑层
│   ├── Constants/              # 常量定义
│   ├── DTOs/                   # 请求/响应模型
│   ├── Jobs/                   # Quartz 任务
│   ├── Services/               # 业务服务
│   └── Validators/             # FluentValidation 验证器
├── By3.Repository/             # 数据访问层
│   ├── AppDbContext.cs         # EF Core 数据库上下文
│   ├── Data/                   # 种子数据初始化
│   ├── Entities/               # 实体类
│   └── Repositories/           # 仓储类
├── By3.Tests/                  # 单元测试
├── By3.IntegrationTests/       # 集成测试
└── By3.slnx                    # 解决方案文件
```

## 业务分层

- **By3.Api**：仅负责接收 HTTP 请求、路由、认证、过滤器，不直接操作数据库。
- **By3.Service**：处理业务逻辑，调用 Repository 完成数据操作。
- **By3.Repository**：负责实体定义、数据库上下文和仓储实现。

依赖方向：`By3.Api → By3.Service → By3.Repository`，无循环依赖。

## 数据表前缀

数据库中所有表均使用 `by3_` 前缀，例如 `by3_sysuser`、`by3_sysrole`、`by3_sysmenu` 等。
表名映射定义在 `By3.Repository/AppDbContext.cs` 的 `OnModelCreating` 中，通过 `TablePrefix` 配置项可调整前缀。

## 敏感信息保护

- **手机号加密存储**：`SysUser.Phone` 在写入数据库前通过 `DataProtection` 配置项中的密钥进行 AES 加密，查询时自动解密。
- **审计日志脱敏**：`SysAuditLog` 的 `RequestParams`、`RequestBody`、`RequestHeaders` 等字段在记录前会经过脱敏处理，避免记录密码、Token 等敏感信息。
- **ApiSecret 仅展示一次**：对外 API 的 `ApiSecret` 在创建时返回，之后不再明文展示。

## 配置说明

项目使用三层配置：

- `appsettings.json`：通用配置结构和非敏感默认值。
- `appsettings.Development.json`：本地开发默认值，包含数据库连接、JWT Key、数据保护密钥、文件上传路径。
- `appsettings.Production.json`：生产环境配置，敏感值留空，强制通过环境变量注入。

主要配置节：

- `ConnectionStrings:DefaultConnection`：PostgreSQL 连接字符串。
- `Jwt`：JWT 密钥、签发人、受众、Token 过期时间。
- `Cors:AllowedOrigins`：允许跨域来源。
- `RateLimiting`：限流参数。
- `FileUpload`：请求体/文件大小限制。
- `FileStorage:UploadPath`：上传文件物理存储路径。
- `DataProtection:EncryptionKey`：敏感数据 AES 加密密钥。
- `Jobs:UserSeed:DefaultPassword`：用户种子任务默认密码。
- `Swagger:IsEnabled`：是否启用 Swagger UI。默认 `Development` 环境启用，`Production` 环境关闭；也可通过环境变量 `Swagger__IsEnabled=true/false` 显式覆盖。
- `TablePrefix`：数据库表前缀，默认 `by3_`，生成的表名为 `{prefix}{表名}`，例如 `by3_sysuser`。

**注意：生产环境敏感配置（连接字符串、JWT Key、数据保护密钥、文件上传路径等）请通过环境变量或 User Secrets 提供，不要提交到代码仓库。**

环境变量示例：

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=by3;Username=postgres;Password=your_password"
export Jwt__Key="your-super-secret-key-at-least-32-bytes-long!"
export Jwt__Issuer="By3"
export Jwt__Audience="By3Client"
export DataProtection__EncryptionKey="your-32-bytes-encryption-key!"
export Cors__AllowedOrigins="http://localhost"
```

Windows CMD 示例：

```cmd
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=by3_dev;Username=postgres;Password=123456
set Jwt__Key=your-super-secret-key-at-least-32-bytes-long!
set DataProtection__EncryptionKey=By3DevPhoneEncryptionKey-ChangeInProd!
```

### 数据库初始化（手动 SQL）

本项目使用手动 SQL 脚本维护数据库结构，不再使用 EF Core 迁移。

最终版脚本位于 `database/migrations/V001__init_schema.sql`，包含完整的建表语句及所有表/字段中文注释。

首次部署或重建数据库时，请手动执行：

```bash
# 创建数据库
psql -h localhost -p 5432 -U postgres -c "CREATE DATABASE by3_dev;"

# 执行初始化脚本
psql -h localhost -p 5432 -U postgres -d by3_dev -f database/migrations/V001__init_schema.sql
```

### 历史数据库升级

如果数据库已使用旧版 `by3sys*`（无下划线）表名初始化，需先手动执行重命名操作：

```sql
-- 示例：重命名用户表及其约束、索引
ALTER TABLE by3sysuser RENAME TO by3_sysuser;
ALTER TABLE by3_sysuser RENAME CONSTRAINT "PK_by3sysuser" TO "PK_by3_sysuser";
-- 其他表、约束、索引按相同规则处理
```

建议新建数据库并重新导入 V001 脚本，以获得完整的字段注释。

启动时会根据 `Database:AutoSeed` 配置决定是否初始化种子数据：

- `Database:AutoSeed: true`：自动插入超级管理员、默认角色、菜单、字典等种子数据。
- `Database:AutoSeed: false`：跳过种子数据初始化。

默认账户：

- 超级管理员：`admin` / `Demo123!`
- 默认角色：超级管理员
- 默认菜单：系统管理、文件管理、邮件管理、日志管理、系统设置、任务管理、对外 API 及其子菜单

> 注意：`Database:AutoMigrate` 已统一设置为 `false`，程序启动时不会执行 EF Core 迁移。

## 运行方式

```bash
# 还原并编译
cd backend
dotnet build By3.slnx

# 运行 API（Development 环境，默认端口 5000）
dotnet run --project By3.Api

# 运行 API（Production 环境，需配置环境变量）
set ASPNETCORE_ENVIRONMENT=Production
dotnet run --project By3.Api --no-launch-profile
```

启动后访问：

- Swagger UI：`http://localhost:5000/swagger`
- 登录接口：`POST http://localhost:5000/api/v1/auth/login`

### 关于 `launchSettings.json`

`Properties/launchSettings.json` 仅在本机使用 `dotnet run` / VS / VS Code 启动时生效，用于配置开发环境的启动 URL 和环境变量：

- `http` profile：监听 `http://localhost:5000`。
- `https` profile：同时监听 `https://localhost:7000` 和 `http://localhost:5000`。

**反向代理场景**：通常保持应用监听 `http://localhost:5000` 不变，由 Nginx / Traefik 等反向代理将外部 80/443 端口转发到该端口即可，无需调整此文件。

发布到生产环境后该文件不生效，请通过环境变量或容器编排配置运行参数。

## 测试项目说明

后端包含两个测试项目，职责不同：

### `By3.Tests` — 单元测试

- 测试目标：单个类、单个方法，不依赖外部服务。
- 不启动 HTTP 服务，不连接真实数据库。
- 适合验证业务逻辑细节，如密码加密、DTO 映射、算法、工具类等。
- 当前已包含：`AuthServiceTests.cs`。

### `By3.IntegrationTests` — 集成测试

- 测试目标：整个 API 请求链路，从 Controller → Service → Repository → PostgreSQL。
- 通过 `CustomWebApplicationFactory` 在内存中启动真实的 By3.Api 实例。
- 使用独立的测试数据库 `by3_test`，每次测试前会删除并重建。
- 已覆盖：Auth、Users、Roles、Menus、Departments、Positions、DictTypes、DictData、EmailTemplates、Files、AuditLogs、LoginLogs 等 Controller 的接口测试。

### 运行测试

```bash
# 运行单元测试
cd backend
dotnet test By3.Tests

# 运行集成测试（需本地 PostgreSQL 及 by3_test 数据库可访问）
dotnet test By3.IntegrationTests
```

> 注意：切换到手动 SQL 方案后，集成测试通过 `EnsureCreatedAsync()` 根据当前实体模型自动创建测试库表结构，无需手动执行 SQL 脚本。

## 核心功能

- ✅ 用户管理（CRUD、分配角色、启用/禁用、部门/岗位、手机号加密存储）
- ✅ 角色管理（CRUD、分配菜单权限）
- ✅ 菜单管理（树形结构，目录/菜单/按钮三级类型）
- ✅ 部门管理（树形组织机构）
- ✅ 岗位管理
- ✅ 字典管理（类型与数据，支持系统常用字典）
- ✅ 文件管理（单文件/多文件上传、拖动上传、文件类型字典校验）
- ✅ 邮件管理（模板、HTML 内容、收件人/抄送人、发送日志）
- ✅ 系统设置（邮件发送端配置）
- ✅ 任务调度（Quartz 定时任务，支持用户数据种子、备份清理）
- ✅ 对外 API（Token 管理、接口注册、AK/SK 签名认证、幂等/限流/失败封禁）
- ✅ 操作日志与登录日志
- ✅ JWT 登录认证与动态菜单权限
- ✅ 接口幂等性（`Idempotency-Key`）
- ✅ 内存缓存（权限列表、菜单树）

## 接口说明

| 控制器 | 基础路径 | 说明 |
|--------|----------|------|
| AuthController | `/api/v1/auth` | 登录、刷新 Token、登出、获取当前用户信息 |
| UsersController | `/api/v1/users` | 用户 CRUD、角色分配、重置密码 |
| RolesController | `/api/v1/roles` | 角色 CRUD、菜单分配 |
| MenusController | `/api/v1/menus` | 菜单 CRUD |
| DepartmentsController | `/api/v1/departments` | 部门 CRUD |
| PositionsController | `/api/v1/positions` | 岗位 CRUD |
| DictTypesController | `/api/v1/dicttypes` | 字典类型 CRUD |
| DictDataController | `/api/v1/dictdata` | 字典数据 CRUD |
| SingleFilesController | `/api/v1/singlefiles` | 单文件上传 |
| MultiFilesController | `/api/v1/multifiles` | 多文件上传 |
| EmailTemplatesController | `/api/v1/emailtemplates` | 邮件模板 CRUD |
| EmailSettingsController | `/api/v1/emailsettings` | 邮件发送配置 |
| JobsController | `/api/v1/jobs` | 定时任务 CRUD、触发 |
| ExternalApiTokensController | `/api/v1/externalapitokens` | 对外 API Token 管理 |
| ExternalApisController | `/api/v1/externalapis` | 对外 API 接口注册管理 |
| ExternalUsersController | `/api/external/v1/users` | 对外 API 示例：用户列表 |
| ExternalSystemInfoController | `/api/external/v1/systeminfo` | 对外 API 示例：系统包信息 |
| ExternalDepartmentsController | `/api/external/v1/departments` | 对外 API 示例：部门树/部门详情 |
| ExternalPositionsController | `/api/external/v1/positions` | 对外 API 示例：岗位列表/岗位详情 |
| AuditLogsController | `/api/v1/auditlogs` | 操作日志查询 |
| LoginLogsController | `/api/v1/loginlogs` | 登录日志查询 |
| SystemInfoController | `/api/v1/systeminfo` | 系统信息、依赖包列表 |

## 幂等性说明

所有非 GET/HEAD 接口需要在请求头中携带：

```http
Idempotency-Key: <唯一字符串>
```

后端会缓存该 Key 首次成功的响应 10 分钟，重复请求直接返回缓存结果，防止重复执行。

## 主要依赖包

后端核心 NuGet 包及协议：

| 用途 | 包名 | 协议 |
|---|---|---|
| Web API / MVC | `Microsoft.AspNetCore.*` | MIT |
| 身份认证 | `Microsoft.AspNetCore.Identity.*` | Apache-2.0 |
| ORM | `Microsoft.EntityFrameworkCore.*` | MIT |
| PostgreSQL 驱动 | `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL License |
| 密码哈希 | `BCrypt.Net-Next` | BSD-3-Clause |
| 参数校验 | `FluentValidation.AspNetCore` | Apache-2.0 |
| JWT | `Microsoft.IdentityModel.JsonWebTokens` | MIT |
| 邮件发送 | `MailKit` | MIT |
| 定时任务 | `Quartz` / `Quartz.Extensions.Hosting` | Apache-2.0 |
| Excel 处理 | `ClosedXML` | MIT |
| 图像处理 | `SkiaSharp` | MIT |
| API 版本控制 | `Asp.Versioning.*` | MIT |
| 接口文档 | `Swashbuckle.AspNetCore` | MIT |

完整列表可通过首页「系统引入包」或 `SystemInfoController` 接口获取。

## 前端配套

前端位于 `../frontend`，使用 Vue 3 + Vite + Element Plus + Pinia 实现，提供动态路由和按钮级权限控制。
