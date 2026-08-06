# 部署指南

本文档说明 By3 系统的部署方式，包括 Docker Compose 一键部署和手动部署两种方案。

## 环境要求

| 组件 | 版本要求 |
|------|---------|
| .NET SDK | 10.0+ |
| Node.js | 22+ |
| PostgreSQL | 16+ |
| Docker | 24+（可选） |
| Docker Compose | v2+（可选） |

## 方案一：Docker Compose 一键部署

适用于快速启动和生产环境。

### 1. 准备配置文件

编辑 `backend/By3.Api/appsettings.json`，修改以下敏感配置：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=by3;Username=postgres;Password=<强密码>"
  },
  "Jwt": {
    "Key": "<至少32字节的随机密钥>"
  },
  "DataProtection": {
    "EncryptionKey": "<至少32字节的随机密钥>"
  },
  "Jobs": {
    "UserSeed": {
      "DefaultPassword": "<强密码>"
    }
  }
}
```

生成随机密钥：

```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

### 2. 启动服务

```bash
docker compose up -d
```

### 3. 验证服务

```bash
# 检查容器状态
docker compose ps

# 检查后端健康状态
curl http://localhost:5000/api/health

# 访问前端
curl http://localhost
```

### 4. 默认账号

- 用户名：`admin`
- 密码：`Demo123!`

> ⚠️ 首次登录后请立即修改密码。

### 5. 服务端口

| 服务 | 端口 | 说明 |
|------|------|------|
| 前端 | 80 | Nginx 反向代理 |
| 后端 API | 5000 | 映射到容器内 8080 |
| PostgreSQL | 5432 | 数据库 |

### 6. 常用命令

```bash
# 查看日志
docker compose logs -f api
docker compose logs -f postgres

# 重启服务
docker compose restart api

# 停止所有服务
docker compose down

# 停止并删除数据卷
docker compose down -v
```

## 方案二：手动部署

适用于开发环境或需要自定义配置的场景。

### 1. 安装 PostgreSQL

```bash
# Ubuntu/Debian
sudo apt install postgresql-16

# macOS (Homebrew)
brew install postgresql@16

# Windows
# 下载安装 https://www.postgresql.org/download/windows/
```

### 2. 创建数据库

```bash
# 登录 PostgreSQL
sudo -u postgres psql

# 创建数据库
CREATE DATABASE by3_dev;

# 退出
\q
```

### 3. 初始化数据库结构

应用首次启动时会通过 `EnsureCreatedAsync` 自动创建数据库表结构，无需手动执行 SQL。

如需手动初始化（可选）：

```bash
psql -h localhost -p 5432 -U postgres -d by3_dev -f database/migrations/V001__init_schema.sql
```

### 4. 配置后端

编辑 `backend/By3.Api/appsettings.json`，修改数据库连接、密钥等配置：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=by3_dev;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your-super-secret-key-at-least-32-bytes-long!"
  },
  "DataProtection": {
    "EncryptionKey": "your-32-bytes-encryption-key!"
  },
  "FileStorage": {
    "UploadPath": "./uploads"
  },
  "Jobs": {
    "UserSeed": {
      "DefaultPassword": "Demo123!"
    }
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5175"
  }
}
```

> 也可以通过环境变量覆盖配置项（格式：`Section__Key`），例如 `ConnectionStrings__DefaultConnection`。

### 5. 启动后端

```bash
cd backend
dotnet build By3.slnx -c Release
dotnet run --project By3.Api --configuration Release
```

后端默认监听 `http://localhost:5000`。

### 6. 构建并部署前端

```bash
cd frontend
npm install
npm run build
```

构建产物在 `frontend/dist/` 目录，使用 Nginx 托管：

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        root /path/to/frontend/dist;
        index index.html;
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## 方案三：Windows 快速启动

项目根目录提供批处理脚本：

```cmd
# 启动后端
start-backend.bat

# 新开终端，启动前端
start-frontend.bat
```

## 生产环境配置清单

### 必须修改的配置

| 配置项 | 说明 | 修改方式 |
|--------|------|---------|
| `ConnectionStrings__DefaultConnection` | 数据库连接 | appsettings.json 或环境变量覆盖 |
| `Jwt__Key` | JWT 签名密钥（≥32字节） | appsettings.json 或环境变量覆盖 |
| `DataProtection__EncryptionKey` | 数据加密密钥（≥32字节） | appsettings.json 或环境变量覆盖 |
| `Jobs__UserSeed__DefaultPassword` | 种子用户密码 | appsettings.json 或环境变量覆盖 |
| `Cors__AllowedOrigins` | 前端域名 | appsettings.json 或环境变量覆盖 |

### 建议修改的配置

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `TablePrefix` | 数据库表前缀 | `by3_` |
| `Jwt__AccessTokenExpireHours` | Token 过期时间 | 8 小时 |
| `RateLimiting` | 限流参数 | 见 appsettings.json |
| `Swagger__IsEnabled` | 是否启用 Swagger | Production 关闭 |

### 安全加固

1. **数据库**：使用强密码，限制网络访问（仅允许应用服务器 IP）
2. **HTTPS**：在 Nginx 中配置 SSL 证书
3. **防火墙**：仅开放 80/443 端口，PostgreSQL 不对外暴露
4. **日志**：配置日志收集（如 ELK、Grafana Loki）
5. **备份**：定期备份 PostgreSQL 数据

### Nginx HTTPS 配置示例

```nginx
server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;

    location / {
        root /path/to/frontend/dist;
        index index.html;
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name your-domain.com;
    return 301 https://$host$request_uri;
}
```

## 健康检查

后端提供健康检查端点：

```bash
GET http://localhost:5000/api/health
```

响应示例：

```json
{
  "status": "Healthy",
  "timestamp": "2026-08-04T12:00:00Z",
  "checks": [
    { "name": "Database", "status": "Healthy" }
  ]
}
```

可用于 Docker 健康检查、负载均衡器探针、Kubernetes liveness/readiness probe。

## 故障排查

| 问题 | 排查方法 |
|------|---------|
| 数据库连接失败 | 检查 PostgreSQL 是否运行，连接字符串是否正确 |
| JWT 验证失败 | 检查 `Jwt__Key` 是否≥32字节，前后端是否一致 |
| 前端无法访问 API | 检查 Nginx 代理配置，CORS 设置 |
| 文件上传失败 | 检查 `FileStorage__UploadPath` 目录权限 |
| 种子数据未初始化 | 检查 `Database__AutoSeed` 是否为 `true` |
