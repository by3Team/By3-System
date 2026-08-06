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
using Microsoft.AspNetCore.Mvc.Filters;

namespace By3.Api.Filters;

/// <summary>
/// 在模型绑定之前捕获请求体，供审计日志使用。
/// Resource Filter 在模型绑定之前执行，因此可以安全地读取并回绕请求流。
/// </summary>
public class RequestBodyCaptureFilter : IAsyncResourceFilter
{
    private const int MaxBodyLength = 64 * 1024;

    /// <summary>
    /// 在资源执行前捕获请求体内容，存入 HttpContext 供审计日志使用。
    /// </summary>
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!HttpMethods.IsGet(request.Method)
            && !HttpMethods.IsHead(request.Method)
            && request.ContentLength > 0
            && request.Path.StartsWithSegments("/api"))
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (body.Length > MaxBodyLength)
                body = body[..MaxBodyLength] + "\n<!-- 请求体过大，已截断 -->";

            context.HttpContext.Items["AuditRequestBody"] = body;
        }

        await next();
    }
}
