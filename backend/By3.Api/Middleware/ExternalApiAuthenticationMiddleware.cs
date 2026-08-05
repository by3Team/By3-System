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

using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using By3.Repository.Repositories;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Middleware;

/// <summary>
/// 对外 API 签名认证中间件。
/// 负责：AK/SK 签名认证、接口注册校验、幂等性校验、单 AK 限流、连续失败封禁、访问记录。
/// </summary>
public class ExternalApiAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExternalApiAuthenticationMiddleware> _logger;

    // 限流窗口与失败封禁阈值（可后续迁移到配置）
    private static readonly TimeSpan RateWindow = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromHours(24);
    private static readonly int MaxConsecutiveFailures = 5;
    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);

    public ExternalApiAuthenticationMiddleware(RequestDelegate next, ILogger<ExternalApiAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/external/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        var timestampHeader = context.Request.Headers["X-Timestamp"].FirstOrDefault();
        var nonce = context.Request.Headers["X-Nonce"].FirstOrDefault();
        var signature = context.Request.Headers["X-Signature"].FirstOrDefault();
        var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();

        var requestParams = await BuildRequestParamsAsync(context);
        var logDto = new CreateExternalApiAccessLogDto
        {
            ApiKey = apiKey ?? string.Empty,
            RequestPath = path,
            RequestMethod = context.Request.Method,
            RequestParams = JsonSerializer.Serialize(requestParams),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Status = "Failed",
            IdempotencyKey = idempotencyKey
        };

        // 1. 接口注册校验：只有登记在 SysExternalApi 表中的接口才允许被外部访问
        var externalApiRepo = context.RequestServices.GetRequiredService<ExternalApiRepository>();
        var externalPath = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ? path.Substring(4) : path;
        var registeredApi = await externalApiRepo.GetByRouteAsync(externalPath, context.Request.Method);

        if (registeredApi == null)
        {
            logDto.ErrorMessage = "该接口未对外开放";
            await LogAsync(context, logDto);
            await WriteErrorAsync(context, 404, logDto.ErrorMessage);
            return;
        }

        if (!registeredApi.IsEnabled || registeredApi.IsDeleted)
        {
            logDto.ErrorMessage = "该接口已停用";
            await LogAsync(context, logDto);
            await WriteErrorAsync(context, 403, logDto.ErrorMessage);
            return;
        }

        // 2. 认证请求头校验
        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(timestampHeader) ||
            string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(signature))
        {
            logDto.ErrorMessage = "缺少必要的认证请求头（X-Api-Key, X-Timestamp, X-Nonce, X-Signature）";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 401, logDto.ErrorMessage);
            return;
        }

        // 3. 时间戳校验
        if (!long.TryParse(timestampHeader, out var timestamp))
        {
            logDto.ErrorMessage = "X-Timestamp 格式无效";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 401, logDto.ErrorMessage);
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - timestamp) > 300)
        {
            logDto.ErrorMessage = "请求时间戳已过期，请使用当前时间生成签名";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 401, logDto.ErrorMessage);
            return;
        }

        // 4. AK/SK 连续失败封禁校验
        var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
        var failureKey = $"external_api_fail:{apiKey}";
        if (cache.TryGetValue(failureKey, out int failureCount) && failureCount >= MaxConsecutiveFailures)
        {
            logDto.ErrorMessage = "该 ApiKey 因连续失败次数过多已被临时封禁";
            await LogAsync(context, logDto);
            await WriteErrorAsync(context, 403, logDto.ErrorMessage);
            return;
        }

        // 5. AK 有效性校验
        var tokenService = context.RequestServices.GetRequiredService<ExternalApiTokenService>();
        var token = await tokenService.GetByApiKeyAsync(apiKey);

        if (token == null)
        {
            logDto.ErrorMessage = "ApiKey 不存在";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 401, logDto.ErrorMessage);
            return;
        }

        if (!token.IsEnabled || token.IsDeleted)
        {
            logDto.ErrorMessage = "ApiKey 已被禁用或删除";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 403, logDto.ErrorMessage);
            return;
        }

        if (token.ExpireTime.HasValue && token.ExpireTime.Value < DateTime.UtcNow)
        {
            logDto.ErrorMessage = "ApiKey 已过期";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 403, logDto.ErrorMessage);
            return;
        }

        // 5.5 接口访问权限校验：若 Token 配置了 AllowedApiIds，则只能访问指定接口
        if (token.AllowedApiIds.Count > 0 && !token.AllowedApiIds.Contains(registeredApi.Id))
        {
            logDto.ErrorMessage = "该 ApiKey 无权访问此接口";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 403, logDto.ErrorMessage);
            return;
        }

        // 6. 幂等性校验
        if (registeredApi.RequireIdempotency)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                logDto.ErrorMessage = "该接口要求提供 Idempotency-Key 请求头";
                await LogFailureAsync(context, logDto, apiKey);
                await WriteErrorAsync(context, 400, logDto.ErrorMessage);
                return;
            }

            var idemKey = $"external_api_idem:{apiKey}:{idempotencyKey}";
            if (cache.TryGetValue(idemKey, out _))
            {
                logDto.ErrorMessage = "Idempotency-Key 已使用";
                await LogAsync(context, logDto);
                await WriteErrorAsync(context, 409, logDto.ErrorMessage);
                return;
            }
        }

        // 7. 单 AK 限流校验
        if (registeredApi.RateLimitPerSecond > 0)
        {
            var rateKey = $"external_api_rate:{apiKey}:{registeredApi.Id}";
            if (!TryAcquireRateLimit(cache, rateKey, registeredApi.RateLimitPerSecond))
            {
                logDto.ErrorMessage = "请求过于频繁，请降低调用频率";
                await LogAsync(context, logDto);
                await WriteErrorAsync(context, 429, logDto.ErrorMessage);
                return;
            }
        }

        // 8. 签名验证
        var expectedSignature = ExternalApiSignatureHelper.Sign(
            context.Request.Method,
            externalPath,
            timestamp,
            nonce,
            token.ApiSecret,
            requestParams);

        if (!ExternalApiSignatureHelper.Verify(expectedSignature, signature.ToLowerInvariant()))
        {
            logDto.ErrorMessage = "签名验证失败";
            await LogFailureAsync(context, logDto, apiKey);
            await WriteErrorAsync(context, 403, logDto.ErrorMessage);
            return;
        }

        // 9. 记录幂等 Key（仅校验通过的请求才记录，避免无效 Key 占满缓存）
        if (registeredApi.RequireIdempotency && !string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var idemKey = $"external_api_idem:{apiKey}:{idempotencyKey}";
            cache.Set(idemKey, true, IdempotencyWindow);
        }

        // 10. 重置失败计数
        cache.Remove(failureKey);

        logDto.Status = "Success";
        context.Items["ExternalApiKey"] = apiKey;

        await LogAsync(context, logDto);
        await _next(context);
    }

    private static async Task<Dictionary<string, string?>> BuildRequestParamsAsync(HttpContext context)
    {
        var parameters = new Dictionary<string, string?>();

        foreach (var query in context.Request.Query)
        {
            parameters[query.Key] = query.Value.FirstOrDefault();
        }

        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            foreach (var field in form)
            {
                parameters[field.Key] = field.Value.FirstOrDefault();
            }
        }

        return parameters;
    }

    /// <summary>
    /// 简单的固定窗口限流：缓存中记录当前窗口内的请求次数。
    /// </summary>
    private static bool TryAcquireRateLimit(IMemoryCache cache, string key, int limit)
    {
        if (cache.TryGetValue(key, out int count))
        {
            if (count >= limit)
                return false;

            cache.Set(key, count + 1, RateWindow);
            return true;
        }

        cache.Set(key, 1, RateWindow);
        return true;
    }

    private static async Task LogFailureAsync(HttpContext context, CreateExternalApiAccessLogDto dto, string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return;

        try
        {
            var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            var failureKey = $"external_api_fail:{apiKey}";
            var count = cache.TryGetValue(failureKey, out int c) ? c : 0;
            cache.Set(failureKey, count + 1, FailureWindow);
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ExternalApiAuthenticationMiddleware>>();
            logger.LogError(ex, "记录对外 API 失败计数失败");
        }

        await LogAsync(context, dto);
    }

    private static async Task LogAsync(HttpContext context, CreateExternalApiAccessLogDto dto)
    {
        try
        {
            var tokenService = context.RequestServices.GetRequiredService<ExternalApiTokenService>();
            await tokenService.LogAccessAsync(dto);
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ExternalApiAuthenticationMiddleware>>();
            logger.LogError(ex, "记录对外 API 访问日志失败");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var result = ApiResult<object>.Error(message, statusCode);
        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
}
