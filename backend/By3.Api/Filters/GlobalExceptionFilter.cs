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

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using By3.Service.DTOs;

namespace By3.Api.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IHostEnvironment _env;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionFilter"/> class.
    /// </summary>
    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// 捕获未处理异常并返回统一的错误响应。
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        var request = context.HttpContext.Request;
        var requestPath = request.Path + request.QueryString;
        var traceId = context.HttpContext.TraceIdentifier;

        _logger.LogError(
            context.Exception,
            "Unhandled exception TraceId={TraceId} {Method} {Path}",
            traceId,
            request.Method,
            requestPath);

        var exception = context.Exception;
        ObjectResult result;

        switch (exception)
        {
            case ValidationException validationEx:
                var errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                result = new ObjectResult(ApiResult<object>.Error("参数验证失败", 400, errors))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                break;

            case UnauthorizedAccessException:
                result = new ObjectResult(ApiResult<object>.Error("无权访问", 403))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                break;

            case InvalidOperationException:
                result = new ObjectResult(ApiResult<object>.Error(exception.Message, 400))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                break;

            default:
                var message = _env.IsDevelopment()
                    ? $"{exception.Message} (TraceId: {traceId})"
                    : $"服务器内部错误 (TraceId: {traceId})";
                result = new ObjectResult(ApiResult<object>.Error(message, 500))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                break;
        }

        context.Result = result;
        context.ExceptionHandled = true;
    }
}
