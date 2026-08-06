# 环境变量配置指南

本文档列出 By3 系统所有可配置的环境变量。

> **注意**：所有配置集中在 `backend/By3.Api/appsettings.json` 中。环境变量是可选的，可用于覆盖任意配置项。

## 配置优先级

配置加载顺序（后加载的优先级更高）：
1. `appsettings.json` — 默认值
2. 环境变量 — 覆盖，生产环境可选
3. 命令行参数 — 最高优先级

## 后端环境变量

### 数据库连接

| 变量名 | 说明 | 示例 | 必填 |
|--------|------|------|------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL 连接字符串 | `Host=localhost;Port=5432;Database=by3_dev;Username=postgres;Password=your_pwd` | ✅ |

### JWT 认证

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `Jwt__Key` | JWT 签名密钥（≥32字节） | 无 | ✅ |
| `Jwt__Issuer` | JWT 签发人 | `By3` | 否 |
| `Jwt__Audience` | JWT 受众 | `By3Client` | 否 |
| `Jwt__AccessTokenExpireHours` | Access Token 过期时间（小时） | `8` | 否 |
| `Jwt__RefreshTokenExpireDays` | Refresh Token 过期时间（天） | `7` | 否 |

### 跨域配置

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `Cors__AllowedOrigins` | 允许的前端地址（逗号分隔） | `http://localhost:5175` | 否 |

### 数据保护

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `DataProtection__EncryptionKey` | 手机号等敏感数据 AES 加密密钥（≥32字节） | 无 | ✅ |

### 文件上传

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `FileStorage__UploadPath` | 文件上传存储路径 | `./uploads` | 否 |

### 定时任务

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `Jobs__UserSeed__DefaultPassword` | 用户种子任务默认密码 | 无 | ✅ |

### 数据库初始化

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `Database__AutoMigrate` | 是否自动执行 EF Core 迁移 | `false` | 否 |
| `Database__AutoSeed` | 是否自动插入种子数据 | `true` | 否 |

### 表前缀

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `TablePrefix` | 数据库表名前缀 | `by3_` | 否 |

### Swagger

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `Swagger__IsEnabled` | 是否启用 Swagger UI | Development 启用，Production 关闭 | 否 |

### ASP.NET Core 内置

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `ASPNETCORE_ENVIRONMENT` | 运行环境 | `Production` | 否 |
| `ASPNETCORE_URLS` | 监听地址 | `http://localhost:5000` | 否 |

## 前端环境变量

前端环境变量以 `VITE_` 开头，在 `.env.development` / `.env.production` 中配置。

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `VITE_API_BASE_URL` | API 基础路径 | `/api` |
| `VITE_PROXY_TARGET` | 开发代理目标地址 | `http://localhost:5000` |

## Docker Compose 环境变量

Docker Compose 读取环境变量，值可在 `docker-compose.yml` 中直接设置或通过环境变量传入：

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `POSTGRES_PASSWORD` | PostgreSQL 密码 | `123456` |
| `JWT_KEY` | JWT 签名密钥 | `your-super-secret-key-at-least-32-bytes-long!` |
| `DATA_PROTECTION_KEY` | 数据保护密钥 | `By3ProdPhoneEncryptionKey-ChangeMe!` |
| `JOBS_USERSEED_PASSWORD` | 种子用户默认密码 | `Demo123!` |
| `TABLE_PREFIX` | 数据库表前缀 | `by3_` |

## 生产环境部署清单

生产环境需设置以下敏感配置（可直接写入 `appsettings.json`，或通过环境变量覆盖）：

1. `ConnectionStrings__DefaultConnection` — 使用强密码
2. `Jwt__Key` — 至少32字节的随机密钥
3. `DataProtection__EncryptionKey` — 至少32字节的随机密钥
4. `Jobs__UserSeed__DefaultPassword` — 设置强密码
5. `Cors__AllowedOrigins` — 设置实际的前端域名

### 生成随机密钥

```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```
