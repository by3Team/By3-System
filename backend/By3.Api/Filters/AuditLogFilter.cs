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

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Filters;

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogFilter"/> class.
    /// </summary>
    public AuditLogFilter(IServiceScopeFactory scopeFactory, ILogger<AuditLogFilter> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 在 Action 执行前后捕获请求与响应信息，异步写入审计日志。
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var resultContext = await next();
        stopwatch.Stop();

        try
        {
            var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = context.HttpContext.User.Identity?.Name ?? "anonymous";
            var route = context.ActionDescriptor.AttributeRouteInfo?.Template ?? context.HttpContext.Request.Path;
            var controller = context.Controller.GetType().Name;
            var action = context.ActionDescriptor.RouteValues.TryGetValue("action", out var act) ? act : "";

            var request = context.HttpContext.Request;
            var response = resultContext.HttpContext.Response;

            var requestParams = MaskSensitiveFields(JsonSerializer.Serialize(context.ActionArguments));
            var requestBody = context.HttpContext.Items.TryGetValue("AuditRequestBody", out var rawBody)
                ? MaskSensitiveFields(rawBody?.ToString() ?? "")
                : null;
            var requestHeaders = MaskSensitiveFields(CaptureHeaders(request.Headers));
            var responseHeaders = CaptureHeaders(response.Headers);

            var (responseResult, isTruncated) = CaptureResponseResult(resultContext.Result);
            if (isTruncated)
                responseResult = $"{responseResult}\n<!-- 响应体过大，已截断 -->";

            var dto = new CreateAuditLogDto
            {
                UserId = userId != null ? Guid.Parse(userId) : null,
                UserName = userName,
                Action = $"{controller}.{action}",
                Controller = controller,
                RequestPath = route,
                RequestMethod = request.Method,
                RequestParams = Truncate(requestParams, 4000),
                RequestBody = Truncate(requestBody, 8000),
                RequestHeaders = Truncate(requestHeaders, 4000),
                ResponseResult = Truncate(responseResult, 8000),
                ResponseHeaders = Truncate(responseHeaders, 2000),
                StatusCode = response.StatusCode,
                ExceptionMessage = resultContext.Exception?.Message,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request.Headers.UserAgent.ToString()
            };

            // 异步落库，使用独立作用域避免请求作用域释放后访问 DbContext
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var auditService = scope.ServiceProvider.GetRequiredService<AuditLogService>();
                    await auditService.CreateAsync(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit log failed in background");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit log failed");
        }
    }

    private static string CaptureHeaders(IHeaderDictionary headers)
    {
        var dict = new Dictionary<string, string>();
        foreach (var (key, value) in headers)
        {
            var val = value.ToString();
            if (SensitiveHeaderNames.Contains(key, StringComparer.OrdinalIgnoreCase))
                val = MaskHeaderValue(val);
            dict[key] = val;
        }
        return JsonSerializer.Serialize(dict);
    }

    private static string MaskHeaderValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 8) return "***";
        return $"{value[..6]}...***";
    }

    private static (string? result, bool truncated) CaptureResponseResult(IActionResult? actionResult)
    {
        if (actionResult == null) return (null, false);

        if (actionResult is ObjectResult obj)
        {
            var json = JsonSerializer.Serialize(obj.Value);
            return json.Length > 8000 ? (json[..8000], true) : (json, false);
        }

        if (actionResult is StatusCodeResult status)
        {
            return ($"StatusCode: {status.StatusCode}", false);
        }

        return ($"ResultType: {actionResult.GetType().Name}", false);
    }

    private static string MaskSensitiveFields(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var masked = MaskJsonElement(doc.RootElement);
            return JsonSerializer.Serialize(masked);
        }
        catch
        {
            return json;
        }
    }

    private static object? MaskJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = SensitiveFieldNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase)
                        ? "***"
                        : MaskJsonElement(prop.Value);
                }
                return dict;
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(MaskJsonElement).ToList();
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                return element.GetDecimal();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            default:
                return element.GetRawText();
        }
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
        return value.Length > maxLength ? value[..maxLength] : value;
    }

    private static readonly HashSet<string> SensitiveFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "PasswordHash", "OldPassword", "NewPassword", "ConfirmPassword", "Token", "RefreshToken",
        "Phone", "Email", "ApiSecret", "ApiKey"
    };

    private static readonly HashSet<string> SensitiveHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "X-Api-Key"
    };
}
