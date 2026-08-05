// Copyright 2026 By3 Team
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace By3.Service.Services;

public static class ExternalApiSignatureHelper
{
    /// <summary>
    /// 生成对外 API 请求签名。
    /// 签名串格式：METHOD&amp;PATH&amp;TIMESTAMP&amp;NONCE&amp;key1=value1&amp;key2=value2
    /// 参数按键名升序排列，值需 URL 编码。
    /// </summary>
    public static string Sign(
        string method,
        string path,
        long timestamp,
        string nonce,
        string apiSecret,
        Dictionary<string, string?> parameters)
    {
        var sortedParams = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}");

        var paramString = string.Join("&", sortedParams);
        var signString = $"{method.ToUpperInvariant()}&{path}&{timestamp}&{nonce}&{paramString}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// 验证签名是否一致（时间安全比较）。
    /// </summary>
    public static bool Verify(string expectedSignature, string actualSignature)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(actualSignature));
    }
}
