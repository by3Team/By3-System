# 越权访问测试用例

> 需要人工执行：启动后端后，用不同 Token 逐一测试以下场景。

## 前置条件

1. 启动后端：`cd backend && dotnet run --project By3.Api`
2. 获取管理员 Token：`POST /api/v1/auth/login` (admin/admin123)
3. 创建普通测试用户并获取其 Token

## 测试用例

### 水平越权（用户A操作用户B的数据）

```bash
# 用普通用户Token尝试修改其他用户信息
curl -X PUT http://localhost:5000/api/v1/users/{admin_user_id} \
  -H "Authorization: Bearer {普通用户token}" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: test-horizontal-1" \
  -d '{"realName":"被篡改的名字"}'
# 预期：403 Forbidden（需要 user:update 权限）
```

### 垂直越权（普通用户访问管理员接口）

```bash
# 用普通用户Token访问用户管理列表
curl http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer {普通用户token}"
# 预期：403 Forbidden（需要 user:list 权限）

# 用普通用户Token创建角色
curl -X POST http://localhost:5000/api/v1/roles \
  -H "Authorization: Bearer {普通用户token}" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: test-vertical-1" \
  -d '{"roleName":"非法角色"}'
# 预期：403 Forbidden（需要 role:create 权限）

# 用普通用户Token删除菜单
curl -X DELETE http://localhost:5000/api/v1/menus/{menu_id} \
  -H "Authorization: Bearer {普通用户token}" \
  -H "Idempotency-Key: test-vertical-2"
# 预期：403 Forbidden（需要 menu:delete 权限）
```

### 未认证访问

```bash
# 不带Token访问受保护接口
curl http://localhost:5000/api/v1/users
# 预期：401 Unauthorized

# 使用过期/无效Token
curl http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer invalid.token.here"
# 预期：401 Unauthorized
```

### Token 黑名单验证

```bash
# 登录获取Token
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: test-blacklist-1" \
  -d '{"userName":"admin","password":"admin123"}' | jq -r '.data.token')

# 登出（Token加入黑名单）
curl -X POST http://localhost:5000/api/v1/auth/logout \
  -H "Authorization: Bearer $TOKEN"

# 用已登出的Token访问
curl http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $TOKEN"
# 预期：401 Unauthorized（Token已失效）
```

## 结果记录

| 测试场景 | 预期结果 | 实际结果 | 通过 |
|---------|---------|---------|------|
| 水平越权-修改其他用户 | 403 | | |
| 垂直越权-用户列表 | 403 | | |
| 垂直越权-创建角色 | 403 | | |
| 垂直越权-删除菜单 | 403 | | |
| 未认证访问 | 401 | | |
| 无效Token | 401 | | |
| Token黑名单 | 401 | | |
