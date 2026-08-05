# 权限控制

## 功能说明

By3 采用基于 Permission（权限标识）的细粒度授权模型，而非简单的角色判断。用户通过角色间接获得权限，一个用户可拥有多个角色，权限取并集。后端通过 `PermissionAuthorizationHandler` 校验接口访问权限，前端通过 `v-permission` 指令控制按钮显隐。

## 核心概念

| 概念 | 说明 |
|------|------|
| 用户 | 系统使用者，可分配多个角色 |
| 角色 | 权限集合，绑定多个菜单/权限 |
| 菜单 | 支持三级结构：目录 → 菜单 → 按钮 |
| 权限标识 | 菜单的 `Permission` 字段，如 `user:create`、`role:list` |

## 菜单类型

| 类型值 | 说明 | 示例 |
|--------|------|------|
| 1 | 目录 | 系统管理、日志管理 |
| 2 | 菜单（页面） | 用户管理、角色管理 |
| 3 | 按钮（操作） | 新增、编辑、删除按钮 |

## 权限校验流程

```
用户登录 → 获取角色列表 → 查询角色绑定的菜单 → 提取权限标识列表 → 写入 JWT Token
                                                        ↓
请求接口 → JWT 解析 → PermissionAuthorizationHandler 检查权限标识 → 允许/拒绝
```

## 接口说明

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/v1/users` | GET | `user:list` | 用户列表 |
| `/api/v1/users` | POST | `user:create` | 创建用户 |
| `/api/v1/users/{id}` | PUT | `user:update` | 更新用户 |
| `/api/v1/users/{id}` | DELETE | `user:delete` | 删除用户 |
| `/api/v1/roles` | GET | `role:list` | 角色列表 |
| `/api/v1/menus` | GET | `menu:list` | 菜单列表 |
| `/api/v1/departments` | GET | `dept:list` | 部门列表 |
| `/api/v1/positions` | GET | `position:list` | 岗位列表 |

## 前端使用

### 按钮级权限控制

```vue
<template>
  <!-- 只有拥有 user:create 权限的用户才能看到此按钮 -->
  <el-button v-permission="'user:create'" type="primary">新增用户</el-button>
  
  <!-- 没有权限时按钮会直接从 DOM 移除 -->
  <el-button v-permission="'user:delete'" type="danger">删除</el-button>
</template>
```

### 路由权限控制

路由配置中的 `meta.permission` 字段用于页面级权限控制，无权限时自动跳转 403 页面。

## 配置步骤

1. 在【菜单管理】中创建目录、菜单、按钮，并填写权限标识（如 `user:create`）
2. 在【角色管理】中创建角色，绑定所需菜单
3. 在【用户管理】中为用户分配角色
4. 前端按钮使用 `v-permission="'user:create'"` 控制显隐
