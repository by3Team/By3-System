# By3 前端项目

本项目是 By3 管理系统的前端部分，基于 Vue 3 + TypeScript + Vite + Element Plus 构建。

## 技术栈

- **Vue 3**：组合式 API + `<script setup>`
- **TypeScript**：类型安全
- **Vite**：构建与开发服务器
- **Element Plus**：UI 组件库
- **Pinia**：状态管理
- **Vue Router**：前端路由
- **Axios**：HTTP 请求
- **Marked**：Markdown 渲染

## 目录结构

```
frontend/
├── public/                 # 静态资源
├── src/
│   ├── api/                # 接口请求封装
│   │   ├── index.ts        # 各模块 API 定义
│   │   └── request.ts      # Axios 封装、拦截器
│   ├── assets/             # 样式、图片等资源
│   ├── components/         # 公共组件
│   │   ├── AppHeader.vue   # 顶部导航栏
│   │   ├── Sidebar.vue     # 侧边菜单栏
│   │   ├── Breadcrumb.vue  # 面包屑
│   │   ├── TagsView.vue    # 标签页
│   │   ├── ThemeSettings.vue # 主题设置
│   │   ├── MenuItem.vue    # 菜单项递归组件
│   │   └── upload/         # 上传组件
│   ├── constants/          # 全局常量
│   ├── directives/         # 自定义指令（如权限指令）
│   ├── router/             # Vue Router 配置
│   ├── stores/             # Pinia 状态管理
│   ├── styles/             # 全局样式
│   ├── utils/              # 工具函数
│   ├── views/              # 页面视图
│   │   ├── system/         # 系统管理（用户、角色、菜单、部门、职位、字典、任务、对外API）
│   │   ├── email/          # 邮件管理
│   │   ├── file/           # 文件管理
│   │   ├── log/            # 日志管理（审计日志、登录日志）
│   │   ├── profile/        # 个人中心
│   │   ├── LoginView.vue   # 登录页
│   │   ├── DashboardView.vue # 首页
│   │   ├── LayoutView.vue  # 布局框架
│   │   └── ...
│   ├── App.vue
│   └── main.ts
├── .env.example            # 环境变量示例
├── .env.development        # 开发环境配置
├── .env.production         # 生产环境配置
├── package.json
├── vite.config.ts
├── tsconfig.json
└── README.md
```

## 环境变量

Vite 会根据运行命令自动加载对应的环境文件：

- `npm run dev` → 加载 `.env.development`
- `npm run build` / `npm run preview` → 加载 `.env.production`

已提供的配置文件：

| 文件 | 说明 |
|---|---|
| `.env.example` | 环境变量模板，可复制为开发或生产配置 |
| `.env.development` | 开发环境配置，`/api` 由 Vite 代理到 `http://localhost:5000` |
| `.env.production` | 生产环境配置，`/api` 由 Nginx 反向代理到后端服务 |

### 主要变量

```
VITE_API_BASE_URL=/api
# VITE_PROXY_TARGET=http://localhost:5000
```

| 变量名 | 说明 |
|---|---|
| `VITE_API_BASE_URL` | 后端 API 基础地址 |
| `VITE_PROXY_TARGET` | 开发代理目标（可选，默认 `http://localhost:5000`） |

代理规则配置在 `vite.config.ts` 中，仅在开发模式生效。

## 本地开发

```bash
# 进入前端目录
cd frontend

# 安装依赖（首次）
npm install

# 启动开发服务器
npm run dev
```

开发服务器默认地址：`http://localhost:5175`

后端 API 默认地址：`http://localhost:5000`，通过 `vite.config.ts` 中的代理转发 `/api` 请求。

## 构建

```bash
# 类型检查并打包
npm run build

# 本地预览生产包
npm run preview
```

## 代码规范

```bash
# ESLint 自动修复
npm run lint

# Prettier 格式化
npm run format
```

## 测试

```bash
npm run test
```

## 主要功能模块

| 模块 | 路径 | 说明 |
|---|---|---|
| 首页 | `/dashboard` | 系统版本、功能列表、更新日志 |
| 用户管理 | `/system/user` | 用户 CRUD、角色分配、重置密码 |
| 角色管理 | `/system/role` | 角色 CRUD、菜单权限分配 |
| 菜单管理 | `/system/menu` | 菜单目录/菜单/按钮管理 |
| 部门管理 | `/system/department` | 部门树形管理 |
| 职位管理 | `/system/position` | 职位管理 |
| 字典管理 | `/system/dict` | 字典类型与字典数据 |
| 文件管理 | `/file/list` | 单文件/多文件上传、下载 |
| 邮件管理 | `/email/template` | 邮件模板、版本、发送测试 |
| 任务管理 | `/system/task` | Quartz 定时任务配置与日志 |
| 对外 API Token | `/externalapi/token` | API Token 生成与签名认证 |
| 对外 API 接口 | `/externalapi/api` | 对外开放接口注册、限流与幂等配置 |
| 审计日志 | `/log/audit` | 操作日志查询 |
| 登录日志 | `/log/login` | 登录日志查询 |
| 个人中心 | `/profile` | 个人信息、修改密码 |

## 后端接口

前端默认通过 `VITE_API_BASE_URL` 访问后端，接口统一以 `/v1/` 开头。

详细接口定义见 `src/api/index.ts`。

## 首页功能文档

首页「系统功能」模块展示的内容来自 `public/docs/features/` 目录下的 Markdown 文件。
新增功能时，在该目录添加 Markdown 文件，并在 `src/views/DashboardView.vue` 的功能列表中配置对应条目。
功能文档会在浏览器新标签页中打开展示。
