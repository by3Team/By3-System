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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace By3.Service.Services;

/// <summary>
/// 敏感数据保护服务：提供 AES 加密/解密及常见掩码能力。
/// 密钥优先从环境变量 DataProtection:EncryptionKey 读取，长度不足 32 字节时循环补齐。
/// </summary>
public class DataProtectionService
{
    private readonly byte[] _key;
    private readonly ILogger<DataProtectionService> _logger;

    public DataProtectionService(IConfiguration configuration, ILogger<DataProtectionService> logger)
    {
        _logger = logger;
        var keyString = configuration["DataProtection:EncryptionKey"];
        if (string.IsNullOrWhiteSpace(keyString))
            throw new InvalidOperationException("DataProtection:EncryptionKey 未配置，请在环境变量或 User Secrets 中设置。");

        // 确保密钥为 32 字节（AES-256）
        var normalized = new StringBuilder();
        while (normalized.Length < 32)
            normalized.Append(keyString);
        _key = Encoding.UTF8.GetBytes(normalized.ToString(0, 32));
    }

    /// <summary>
    /// AES-256-CBC 加密，IV 随机生成并附加在密文前。
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// AES-256-CBC 解密。如果数据本身为明文或格式错误，返回 null。
    /// </summary>
    public string? Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var full = Convert.FromBase64String(cipherText);
            if (full.Length <= 16)
                return null;

            using var aes = Aes.Create();
            aes.Key = _key;
            var iv = new byte[16];
            Buffer.BlockCopy(full, 0, iv, 0, 16);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(full, 16, full.Length - 16);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "解密失败，可能为明文数据：{CipherTextPreview}", cipherText?[..Math.Min(cipherText.Length, 20)]);
            return null;
        }
    }

    /// <summary>
    /// 判断一段文本是否看起来已被加密（Base64 且长度合理）。
    /// </summary>
    public bool IsEncrypted(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        if (value.Length < 32)
            return false;
        try
        {
            var bytes = Convert.FromBase64String(value);
            return bytes.Length > 16;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 手机号掩码：保留前 3 位和后 4 位，中间用 **** 替代。
    /// </summary>
    public string MaskPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return string.Empty;
        if (phone.Length < 7)
            return new string('*', phone.Length);
        return phone[..3] + "****" + phone[^4..];
    }

    /// <summary>
    /// 邮箱掩码：@ 前保留首尾各 1 位，@ 后保留域名首字母和后缀。
    /// </summary>
    public string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return email ?? string.Empty;

        var parts = email.Split('@', 2);
        var local = parts[0];
        var domain = parts[1];

        var maskedLocal = local.Length <= 2
            ? new string('*', local.Length)
            : local[..1] + "***" + local[^1..];

        return $"{maskedLocal}@{domain}";
    }

    /// <summary>
    /// 通用敏感文本掩码：保留首尾各 2 位，中间替换为 ****。
    /// </summary>
    public string MaskSensitive(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (value.Length <= 4)
            return new string('*', value.Length);
        return value[..2] + "****" + value[^2..];
    }
}
