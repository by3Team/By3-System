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

namespace By3.Service.DTOs;

/// <summary>
/// 审计日志查询条件。
/// </summary>
public class AuditLogQueryDto
{
    public string? UserName { get; set; }
    public string? Keyword { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 创建审计日志请求（内部使用）。
/// </summary>
public class CreateAuditLogDto
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Controller { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestParams { get; set; }
    public string? RequestBody { get; set; }
    public string? RequestHeaders { get; set; }
    public string? ResponseResult { get; set; }
    public string? ResponseHeaders { get; set; }
    public int? StatusCode { get; set; }
    public string? ExceptionMessage { get; set; }
    public long ElapsedMs { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// 审计日志列表响应。
/// </summary>
public class AuditLogListDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public long ElapsedMs { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasDetail { get; set; }
}

/// <summary>
/// 审计日志详情响应。
/// </summary>
public class AuditLogDetailDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Controller { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestParams { get; set; }
    public string? RequestBody { get; set; }
    public string? RequestHeaders { get; set; }
    public string? ResponseResult { get; set; }
    public string? ResponseHeaders { get; set; }
    public int? StatusCode { get; set; }
    public string? ExceptionMessage { get; set; }
    public long ElapsedMs { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
