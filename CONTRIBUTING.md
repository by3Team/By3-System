# Contributing to By3

感谢您对 By3 RBAC 管理系统的关注！本文档说明如何参与贡献。

## 开发环境

- .NET 10 SDK
- Node.js 22+
- PostgreSQL 16
- Git

## 快速开始

```bash
# 克隆仓库
git clone <your-fork-url>
cd By3-System

# 后端（配置项在 backend/By3.Api/appsettings.json 中，首次启动自动建库建表）
cd backend
dotnet build By3.slnx
dotnet run --project By3.Api

# 前端（新终端）
cd frontend
npm install
npm run dev
```

## 分支策略

- `main` — 稳定版本，仅通过 PR 合入
- `dev` — 开发分支，日常开发基于此分支
- `feat/*` — 功能分支，从 `dev` 创建
- `fix/*` — 修复分支，从 `dev` 创建

## 提交规范

使用 [Conventional Commits](https://www.conventionalcommits.org/) 格式：

```
<type>(<scope>): <description>

[optional body]
[optional footer]
```

常用类型：
- `feat` — 新功能
- `fix` — 修复
- `docs` — 文档
- `style` — 格式调整（不影响逻辑）
- `refactor` — 重构
- `test` — 测试
- `chore` — 构建/工具/依赖

示例：
```
feat(user): 添加用户批量导入功能
fix(auth): 修复 Token 刷新时并发竞态问题
docs(api): 更新 Swagger 注释
```

## Pull Request 流程

1. Fork 仓库并创建功能分支
2. 确保代码通过所有检查：
   ```bash
   # 后端
   cd backend
   dotnet build By3.slnx
   dotnet test By3.Tests
   dotnet test By3.IntegrationTests  # 需要本地 PostgreSQL

   # 前端
   cd frontend
   npm run lint
   npm run test
   npm run build
   ```
3. 提交 PR，填写清晰的描述
4. 等待 CI 通过并请求 Review
5. 根据反馈修改，再次推送

## 代码规范

### 后端 (C#)

- 遵循项目 `.editorconfig` 配置
- Controller 仅负责路由和参数接收，业务逻辑放在 Service 层
- 所有非 GET/HEAD 接口需要 `Idempotency-Key` 请求头
- 敏感数据（手机号等）必须加密存储
- 审计日志字段自动脱敏，不要手动记录密码/Token

### 前端 (Vue 3 + TypeScript)

- 使用 Composition API + `<script setup>`
- API 调用统一通过 `src/api/request.ts` 封装
- 路径别名 `@` 指向 `src/`
- 权限控制使用 `src/directives/` 中的指令

## 数据库变更

项目启动时通过 EF Core `EnsureCreatedAsync()` 自动创建数据库和表结构，无需手动执行 SQL。

手动 SQL 脚本仍保留在 `database/migrations/` 中，用于参考表结构或手动建表：

- 命名格式：`V{序号}__{描述}.sql`
- 新增表或字段需同步更新 `docs/database-schema.md`

## 安全相关

- 不要提交任何密钥、密码或 Token
- 所有用户输入必须验证和参数化（防 SQL 注入）
- 新增 `v-html`、文件读写、反序列化等操作需额外安全审查
- 提交前运行安全扫描：
  ```bash
  # Linux/macOS/Git Bash
  ./scripts/security-scan.sh

  # Windows
  scripts\security-scan.bat
  ```

## 报告问题

使用 GitHub Issues 报告 bug，请包含：

- 重现步骤
- 期望行为 vs 实际行为
- 环境信息（OS、.NET 版本、Node 版本、PostgreSQL 版本）
- 相关日志或截图

## 行为准则

- 尊重所有参与者
- 建设性地讨论技术问题
- 接受合理的不同意见

## 许可证

贡献代码将按照 [Apache License 2.0](LICENSE) 发布。
