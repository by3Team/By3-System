# 对外 API

## 功能说明

By3 对外提供一组受 HMAC-SHA256 签名保护的 Open API，方便第三方系统安全地访问平台数据，无需维护复杂的登录态。

平台侧通过「对外 API > 接口管理」维护允许被外部访问的接口清单；通过「Token 管理」为每个第三方应用颁发独立的 ApiKey / ApiSecret。

## 核心能力

- **ApiKey + ApiSecret**：后台生成一对访问密钥，可设置有效期与启用状态。
- **接口注册管控**：只有登记在「接口管理」中且启用的接口，才允许被外部调用。
- **HMAC-SHA256 签名**：请求需携带 `X-Api-Key`、`X-Timestamp`、`X-Nonce`、`X-Signature` 四个请求头。
- **防重放**：Nonce 10 分钟内不可重复使用，时间戳与服务器时间偏差超过 5 分钟拒绝请求。
- **幂等性**：接口可配置是否要求 `Idempotency-Key`，重复 Key 直接拒绝，保证写入类操作不重复执行。
- **限流**：可按接口为每个 ApiKey 设置每秒最大请求数（QPS）。
- **失败封禁**：单个 ApiKey 连续失败 5 次后，15 分钟内会被临时封禁。
- **访问日志**：自动记录每次对外 API 调用的路径、参数、幂等 Key、结果与异常信息。
- **重新生成与缓冲期**：Token 可重新生成 Key/Secret。重新生成时可选择旧 Key「立即失效」或「指定时间后失效」；同一应用最多同时存在两个有效 Key（当前 Key 与一个缓冲中的旧 Key），更早的旧 Key 会自动失效。

## 签名规则

签名串格式：

```
METHOD&PATH&TIMESTAMP&NONCE&key1=value1&key2=value2
```

- `PATH` 为对外路径，例如 `/external/v1/users`，不是完整的 `/api/external/v1/users`。
- 参数按键名升序排列，值需 URL 编码。
- 整体使用 HMAC-SHA256（密钥为 ApiSecret）生成签名。

## 使用方式

1. 在【对外 API > 接口管理】中确认目标接口已注册并启用。
2. 在【对外 API > Token 管理】中新增 Token，获取 ApiKey 与 ApiSecret。
3. 参考 Token 页面中的 C# / JavaScript 调用示例实现签名逻辑。
4. 调用示例接口：
   - `GET /api/external/v1/users`
   - `GET /api/external/v1/systeminfo/packages`
   - `GET /api/external/v1/departments`
   - `GET /api/external/v1/departments/{id}`
   - `GET /api/external/v1/positions`
   - `GET /api/external/v1/positions/{id}`

## Demo 请求示例（获取部门树）

假设 ApiKey / ApiSecret 已创建，请求 `GET /api/external/v1/departments`：

```http
GET /api/external/v1/departments HTTP/1.1
Host: localhost:5000
X-Api-Key: <ApiKey>
X-Timestamp: 1785744869
X-Nonce: 1113b6c722bd4d03afc941adc15a380f
X-Signature: 296c943144955a0d58b05d9e529af92cbda6b808d9a63faf5f5dd2c3a1ac8d12
```

签名串：

```
GET&/external/v1/departments&1785744869&1113b6c722bd4d03afc941adc15a380f&
```

Python 签名生成：

```python
import hmac, hashlib, time, uuid

api_secret = 'your_api_secret'
path = '/external/v1/departments'
method = 'GET'
timestamp = str(int(time.time()))
nonce = uuid.uuid4().hex
sign_string = f'{method.upper()}&{path}&{timestamp}&{nonce}&'
signature = hmac.new(api_secret.encode(), sign_string.encode(), hashlib.sha256).hexdigest()
```

响应：

```json
{
  "code": 200,
  "message": "success",
  "data": []
}
```

后端 Demo 接口的完整说明可参考：
`backend/By3.Api/Controllers/External/README.md`

## 安全建议

- ApiSecret 仅展示一次，请妥善保存。
- 定期更换 Token 并设置合理的有效期。
- 生产环境应对外 API 启用 HTTPS。
- 写入类接口建议开启幂等校验，防止网络重试导致重复执行。
