# 登录认证

## 功能说明

系统采用 JWT 无状态认证机制。用户登录后颁发 Access Token 和 Refresh Token，前端自动携带，后端无状态校验。

## 核心能力

| 能力 | 说明 |
|------|------|
| JWT 登录 | 用户名密码验证通过后返回 Token 和用户信息 |
| Token 刷新 | Access Token 过期后，通过 Refresh Token 无感续期 |
| Token 黑名单 | 登出或改密后，Token 立即失效 |
| 登录失败锁定 | 5 分钟内连续失败 5 次，自动锁定 15 分钟 |
| 首次改密 | 新用户首次登录标记 `NeedChangePassword`，前端引导修改密码 |
| 权限缓存 | 用户权限和菜单缓存 5 分钟，减少数据库查询 |
| 登录日志 | 自动记录每次登录的用户名、IP、成功/失败状态 |

## 配置项

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| `Jwt__Key` | 无 | 签名密钥，≥32 字节，**必须设置** |
| `Jwt__Issuer` | By3 | 签发人 |
| `Jwt__Audience` | By3Client | 受众 |
| `Jwt__AccessTokenExpireHours` | 8 | Access Token 有效期（小时） |
| `Jwt__RefreshTokenExpireDays` | 7 | Refresh Token 有效期（天） |

## 安全策略

- 密码使用 BCrypt 哈希存储，不可逆
- `ClockSkew` 设为 `Zero`，过期即失效
- 登录接口单独限流（10 次/分钟），防暴力破解
- 登出后 Token 立即失效（内存黑名单）
