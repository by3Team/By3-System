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

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using By3.Service.DTOs;

namespace By3.Api.Filters;

public class IdempotencyFilter : IAsyncActionFilter
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IdempotencyFilter> _logger;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyFilter"/> class.
    /// </summary>
    public IdempotencyFilter(IMemoryCache cache, ILogger<IdempotencyFilter> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 在 Action 执行前检查幂等性 Key，防止重复提交。
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method) ||
            HttpMethods.IsHead(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues))
        {
            context.Result = new BadRequestObjectResult(ApiResult<object>.Error("缺少 Idempotency-Key 头", 400));
            return;
        }

        string? key = keyValues.FirstOrDefault();
        if (string.IsNullOrEmpty(key))
        {
            context.Result = new BadRequestObjectResult(ApiResult<object>.Error("Idempotency-Key 不能为空", 400));
            return;
        }

        var userId = context.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var path = context.HttpContext.Request.Path.Value ?? "";
        var cacheKey = $"idempotent:{userId}:{path}:{key}";
        var lockKey = $"lock:{cacheKey}";

        // 使用轻量锁防止并发执行同一 Key
        var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            if (_cache.TryGetValue(cacheKey, out object? cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("幂等命中，Key: {Key}", key);
                context.Result = (IActionResult)cachedResult;
                return;
            }

            var resultContext = await next();
            if (resultContext.Result is ObjectResult objResult && objResult.StatusCode >= 200 && objResult.StatusCode < 300)
            {
                _cache.Set(cacheKey, objResult, TimeSpan.FromMinutes(10));
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
