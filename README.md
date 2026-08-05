# By3 管理系统

基于 ASP.NET Core 10 + Vue 3 的 By3 权限管理系统。

## 技术栈

### 后端
- ASP.NET Core 10
- Entity Framework Core 9 + Npgsql（PostgreSQL）
- JWT 认证
- FluentValidation
- IMemoryCache
- Quartz.NET（定时任务）
- MailKit（邮件发送）
- xUnit 单元测试 / 集成测试

### 前端
- Vue 3 + Vite
- Element Plus
- Pinia
- Vue Router 4
- Vitest

## 系统架构

```
┌─────────────┐      HTTP       ┌─────────────┐
│   前端      │ ◄──────────────► │  后端 API   │
│  Vue 3      │    /api/v1/*    │ ASP.NET Core│
└─────────────┘                 └──────┬──────┘
                                       │
                                ┌──────┴──────┐
                                │ PostgreSQL  │
                                │  by3_* 表   │
                                └─────────────┘
```

分层依赖：`frontend` → `By3.Api` → `By3.Service` → `By3.Repository` → `PostgreSQL`。

## 快速开始

### 环境要求
- .NET 10 SDK
- Node.js 22+
- PostgreSQL 16

### 默认端口

| 服务 | 地址 | 说明 |
|---|---|---|
| 前端开发服务器 | `http://localhost:5175` | Vite dev server |
| 后端 API | `http://localhost:5000` | Kestrel / launchSettings http profile |
| Swagger UI | `http://localhost:5000/swagger` | API 文档 |

### 环境变量

复制 `.env.example` 为 `.env`，或直接在 shell 中设置：

```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=by3_dev;Username=postgres;Password=your_password"
export Jwt__Key="your-super-secret-key-at-least-32-bytes-long!"
export DataProtection__EncryptionKey="your-32-bytes-encryption-key!"
export FileStorage__UploadPath="./uploads"
export Jobs__UserSeed__DefaultPassword="Demo123!"
export Cors__AllowedOrigins="http://localhost:5175"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=by3_dev;Username=postgres;Password=your_password"
$env:Jwt__Key="your-super-secret-key-at-least-32-bytes-long!"
$env:DataProtection__EncryptionKey="your-32-bytes-encryption-key!"
$env:FileStorage__UploadPath="./uploads"
$env:Jobs__UserSeed__DefaultPassword="Demo123!"
$env:Cors__AllowedOrigins="http://localhost:5175"
```

### 数据库初始化

本项目采用手动 SQL 脚本管理数据库结构（不使用 EF Core 自动迁移）。启动后端前，请先创建数据库并导入初始表结构：

```bash
# 1. 创建数据库（以 PostgreSQL 为例）
createdb -h localhost -p 5432 -U postgres by3_dev

# 2. 导入初始表结构
psql -h localhost -p 5432 -U postgres -d by3_dev -f database/migrations/V001__init_schema.sql
```

Windows 可使用 pgAdmin 或 psql 命令行完成同等操作。导入完成后，启动后端时会自动初始化种子数据（菜单、角色、字典、默认管理员等）。

### 启动后端

```bash
cd backend
dotnet build By3.slnx
dotnet run --project By3.Api
```

或使用项目根目录下的批处理脚本（Windows）：

```cmd
start-backend.bat
```

### 启动前端

```bash
cd frontend
cp .env.example .env.development
npm install
npm run dev
```

或使用项目根目录下的批处理脚本（Windows）：

```cmd
start-frontend.bat
```

### Docker Compose 启动

```bash
cp .env.example .env
docker compose up -d
```

## 默认账号

- 用户名：`admin`
- 密码：`admin123`

## 项目结构

```
├── backend/              # ASP.NET Core 后端
│   ├── By3.Api/          # Web API 主机
│   ├── By3.Service/      # 业务逻辑层
│   ├── By3.Repository/   # 数据访问层
│   ├── By3.Tests/        # 单元测试
│   ├── By3.IntegrationTests/ # 集成测试
│   └── By3.slnx          # 解决方案文件
├── frontend/             # Vue 3 前端
├── database/migrations/  # 数据库初始化 SQL 脚本
│   └── V001__init_schema.sql
├── docs/                 # 文档
│   └── database-schema.md # 数据库表结构说明
└── .github/workflows/    # CI/CD
```

## 核心功能

- 用户管理、角色管理、菜单管理
- 部门管理、岗位管理
- 字典管理（性别、是否启用、文件类型等系统常用字典）
- 文件管理（单文件/多文件上传、拖动上传、文件类型字典校验）
- 邮件管理（模板、HTML/文本内容、收件人/抄送人、发送日志）
- 系统设置（邮件发送端配置）
- 任务调度（Quartz 定时任务、用户数据种子、备份清理）
- 对外 API（Token 管理、接口注册、AK/SK 签名认证、幂等/限流/失败封禁）
- 一人多角色，权限取并集
- JWT 认证 + 基于 Permission 的授权
- Token 刷新、登出、黑名单
- 动态路由与按钮级权限
- 操作日志、登录日志
- 接口幂等性
- 内存缓存
- 软删除与审计字段
- 敏感信息脱敏与手机号加密存储

## 首页功能文档

系统首页的「系统功能」模块直接调用 `frontend/public/docs/features/` 下的 Markdown 文件，
在浏览器新标签页中展示各项功能说明。已包含的文档：

- `auth.md` — 登录认证
- `rbac.md` — 权限控制
- `idempotency.md` — 接口幂等
- `rate-limit.md` — 限流保护
- `audit.md` — 审计日志
- `login-log.md` — 登录日志
- `file.md` — 文件管理
- `dict.md` — 字典管理
- `email.md` — 邮件管理
- `job.md` — 定时任务
- `external-api.md` — 对外 API
- `organization.md` — 部门岗位
- `api-version.md` — API 版本控制
- `theme.md` — 主题与布局
- `compression.md` — 响应压缩

新增功能时，在该目录下添加同名 Markdown 文件，并在首页功能列表中配置对应条目即可。

## 文档

- [数据库设计](docs/database-schema.md)
- [后端说明](backend/README.md)
- [前端说明](frontend/README.md)
- [定时任务说明](backend/By3.Service/Jobs/README.md)
- [对外 API Demo 接口说明](backend/By3.Api/Controllers/External/README.md)
