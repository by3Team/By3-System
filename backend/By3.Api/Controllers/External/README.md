# 对外 API Demo 接口说明

本目录存放所有对外开放的 API 控制器。外部系统通过 AK/SK 签名方式访问，无需登录获取 JWT。

---

# 一、认证与签名规则

## 1. 认证流程

1. 在 By3 系统前端「对外 API → Token 管理」中创建 Token，获取 `ApiKey` 和 `ApiSecret`。
2. 在「对外 API → 接口管理」中确认目标接口已注册并启用。
3. 在「Token 管理」中为该 Token 配置「可访问接口」。不选择任何接口时，Token 可访问所有已启用的对外接口；选择后仅允许访问指定接口。
4. 外部请求时携带以下请求头：

| 请求头 | 说明 |
|---|---|
| `X-Api-Key` | 创建 Token 时获得的 ApiKey |
| `X-Timestamp` | Unix 时间戳（秒） |
| `X-Nonce` | 随机字符串，10 分钟内不可重复 |
| `X-Signature` | HMAC-SHA256 签名（小写十六进制） |
| `Idempotency-Key` | 幂等性 Key（仅当接口注册时开启幂等校验时需要） |

## 2. 签名算法

签名串格式：

```
METHOD&PATH&TIMESTAMP&NONCE&PARAMS
```

说明：

- `METHOD`：HTTP 方法大写，例如 `GET`、`POST`。
- `PATH`：对外路径，例如 `/external/v1/departments`，**不是** `/api/external/v1/departments`。
- `TIMESTAMP`：`X-Timestamp` 的值。
- `NONCE`：`X-Nonce` 的值。
- `PARAMS`：查询参数按键名升序排列，格式为 `key1=value1&key2=value2`，值需 URL 编码；无参数时为空字符串。

使用 `ApiSecret` 对签名串做 HMAC-SHA256 运算，得到 `X-Signature`。

### Python 签名示例

```python
import hmac, hashlib, urllib.parse, time, uuid

api_secret = 'your_api_secret'
path = '/external/v1/departments'
method = 'GET'
timestamp = str(int(time.time()))
nonce = uuid.uuid4().hex
params = {}  # 如果有查询参数，例如 {'page': '1', 'pageSize': '10'}

sorted_params = sorted(
    [(k, urllib.parse.quote(str(v))) for k, v in params.items() if v is not None and v != ''],
    key=lambda x: x[0]
)
param_string = '&'.join([f'{k}={v}' for k, v in sorted_params])
sign_string = f'{method.upper()}&{path}&{timestamp}&{nonce}&{param_string}'
signature = hmac.new(api_secret.encode(), sign_string.encode(), hashlib.sha256).hexdigest()

print('X-Timestamp:', timestamp)
print('X-Nonce:', nonce)
print('X-Signature:', signature)
```

### C# 签名示例

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Web;

string apiSecret = "your_api_secret";
string path = "/external/v1/departments";
string method = "GET";
long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
string nonce = Guid.NewGuid().ToString("N");
var parameters = new Dictionary<string, string?>();

var sortedParams = parameters
    .Where(p => !string.IsNullOrEmpty(p.Value))
    .OrderBy(p => p.Key, StringComparer.Ordinal)
    .Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}");

string paramString = string.Join("&", sortedParams);
string signString = $"{method.ToUpperInvariant()}&{path}&{timestamp}&{nonce}&{paramString}";

using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
string signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signString))).ToLowerInvariant();
```

---

# 二、Demo 接口列表

## 1. 获取部门树

- **对外路径**：`/external/v1/departments`
- **实际请求 URL**：`GET http://localhost:5000/api/external/v1/departments`
- **说明**：获取全部部门，以树形结构返回。

### 请求示例

```http
GET /api/external/v1/departments HTTP/1.1
Host: localhost:5000
X-Api-Key: by3_08d4ad37917d8c40b7bbae526f81f39c
X-Timestamp: 1785744869
X-Nonce: 1113b6c722bd4d03afc941adc15a380f
X-Signature: 296c943144955a0d58b05d9e529af92cbda6b808d9a63faf5f5dd2c3a1ac8d12
```

Python 签名生成：

```python
import hmac, hashlib, urllib.parse, time, uuid

api_key = '<your_api_key>'
api_secret = '<your_api_secret>'
path = '/external/v1/departments'
method = 'GET'
timestamp = str(int(time.time()))
nonce = uuid.uuid4().hex

# GET 请求无参数时，参数串为空字符串
sign_string = f'{method.upper()}&{path}&{timestamp}&{nonce}&'
signature = hmac.new(api_secret.encode(), sign_string.encode(), hashlib.sha256).hexdigest()
print(timestamp, nonce, signature)
```

### 响应示例

```json
{
  "code": 200,
  "message": "success",
  "data": []
}
```

---

## 2. 获取部门详情

- **对外路径**：`/external/v1/departments/{id}`
- **实际请求 URL**：`GET http://localhost:5000/api/external/v1/departments/{id}`
- **说明**：根据部门 ID 获取单个部门信息。

### 请求示例

```http
GET /api/external/v1/departments/11111111-1111-1111-1111-111111111111 HTTP/1.1
Host: localhost:5000
X-Api-Key: by3_08d4ad37917d8c40b7bbae526f81f39c
X-Timestamp: 1785744869
X-Nonce: 1113b6c722bd4d03afc941adc15a380f
X-Signature: <签名>
```

签名串：

```
GET&/external/v1/departments/11111111-1111-1111-1111-111111111111&<timestamp>&<nonce>&
```

---

## 3. 获取岗位分页列表

- **对外路径**：`/external/v1/positions`
- **实际请求 URL**：`GET http://localhost:5000/api/external/v1/positions?page=1&pageSize=10`
- **说明**：分页查询岗位列表。

### 请求示例

```http
GET /api/external/v1/positions?page=1&pageSize=10 HTTP/1.1
Host: localhost:5000
X-Api-Key: by3_08d4ad37917d8c40b7bbae526f81f39c
X-Timestamp: 1785744901
X-Nonce: 4a450fade1ff4505bf48c3a2c97f22a7
X-Signature: 86f5f306fadc4f5748ed96d7525d4857b14641f7dfc989a3dd266a482c1e001a
```

Python 签名生成：

```python
import hmac, hashlib, urllib.parse, time, uuid

api_key = '<your_api_key>'
api_secret = '<your_api_secret>'
path = '/external/v1/positions'
method = 'GET'
timestamp = str(int(time.time()))
nonce = uuid.uuid4().hex
params = {'page': '1', 'pageSize': '10'}

sorted_params = sorted(
    [(k, urllib.parse.quote(str(v))) for k, v in params.items() if v is not None and v != ''],
    key=lambda x: x[0]
)
param_string = '&'.join([f'{k}={v}' for k, v in sorted_params])
sign_string = f'{method.upper()}&{path}&{timestamp}&{nonce}&{param_string}'
signature = hmac.new(api_secret.encode(), sign_string.encode(), hashlib.sha256).hexdigest()
print(timestamp, nonce, signature)
```

签名串示例：

```
GET&/external/v1/positions&1785744901&4a450fade1ff4505bf48c3a2c97f22a7&page=1&pageSize=10
```

### 响应示例

```json
{
  "code": 200,
  "message": "success",
  "data": {
    "total": 0,
    "items": [],
    "page": 1,
    "pageSize": 10
  }
}
```

---

## 4. 获取岗位详情

- **对外路径**：`/external/v1/positions/{id}`
- **实际请求 URL**：`GET http://localhost:5000/api/external/v1/positions/{id}`

### 请求示例

```http
GET /api/external/v1/positions/11111111-1111-1111-1111-111111111111 HTTP/1.1
Host: localhost:5000
X-Api-Key: by3_08d4ad37917d8c40b7bbae526f81f39c
X-Timestamp: 1785744901
X-Nonce: 4a450fade1ff4505bf48c3a2c97f22a7
X-Signature: <签名>
```

---

## 5. 获取用户分页列表

- **对外路径**：`/external/v1/users`
- **实际请求 URL**：`GET http://localhost:5000/api/external/v1/users?page=1&pageSize=10`
- **说明**：分页查询系统用户。

### 请求示例

```http
GET /api/external/v1/users?page=1&pageSize=10 HTTP/1.1
Host: localhost:5000
X-Api-Key: by3_08d4ad37917d8c40b7bbae526f81f39c
X-Timestamp: <timestamp>
X-Nonce: <nonce>
X-Signature: <签名>
```

---

## 6. 获取系统引入包信息

- **对外路径**：`/external/v1/systeminfo/packages`
- **实际请求 URL**：`GET http://localhost:5000/api/external/v1/systeminfo/packages`

### 请求示例

```http
GET /api/external/v1/systeminfo/packages HTTP/1.1
Host: localhost:5000
X-Api-Key: by3_08d4ad37917d8c40b7bbae526f81f39c
X-Timestamp: <timestamp>
X-Nonce: <nonce>
X-Signature: <签名>
```

---

# 三、如何新增对外 Demo 接口

## 步骤 1：实现 Controller

在 `backend/By3.Api/Controllers/External/` 下新建类：

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Controllers.External;

[ApiController]
[Route("api/external/v{version:apiVersion}/demo")]
[ApiVersion("1.0")]
public class ExternalDemoController : ControllerBase
{
    private readonly SomeService _someService;

    public ExternalDemoController(SomeService someService)
    {
        _someService = someService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var result = await _someService.GetListAsync();
        return Ok(ApiResult<object>.Ok(result));
    }
}
```

注意：

- 路由必须以 `api/external/v{version:apiVersion}/` 开头；
- 不需要加 `[Authorize]`，由中间件负责 AK/SK 认证；
- 返回统一使用 `ApiResult<T>.Ok(...)`。

## 步骤 2：在前端注册接口

登录系统后进入 **对外 API → 接口管理**，新增记录：

| 字段 | 示例值 |
|---|---|
| 接口名称 | Demo 列表 |
| 请求路径 | `/external/v1/demo` |
| 请求方法 | GET |
| 限流(QPS) | 10 |
| 需幂等校验 | 否（GET 通常关闭） |
| 启用状态 | 启用 |

只有完成注册后，外部请求才会被中间件放行。

## 步骤 3：验证

使用已创建的 ApiKey / ApiSecret 生成签名并调用：

```bash
curl "http://localhost:5000/api/external/v1/demo" \
  -H "X-Api-Key: <ApiKey>" \
  -H "X-Timestamp: <timestamp>" \
  -H "X-Nonce: <nonce>" \
  -H "X-Signature: <signature>"
```

---

# 四、常见问题

## 1. 返回 "该接口未对外开放"

表示 `by3_sysexternalapi` 表中没有匹配到该路径和方法的记录。请检查：

- 路径是否包含 `/api` 前缀（注册时不应包含，只写 `/external/v1/...`）；
- 方法是否大写（GET/POST/PUT/DELETE）；
- 接口是否已启用且未删除。

## 2. 返回 "签名验证失败"

请检查签名串组成是否正确：

- `PATH` 必须是 `/external/v1/...`，不是 `/api/external/v1/...`；
- 参数按键名升序排列；
- 参数值需 URL 编码；
- 无参数时末尾保留 `&`，即 `METHOD&PATH&TIMESTAMP&NONCE&`。

## 3. 返回 "该接口要求提供 Idempotency-Key"

如果接口在注册时开启了「需幂等校验」，请求头必须携带 `Idempotency-Key`，且同一个 Key 在 24 小时内不能重复使用。
