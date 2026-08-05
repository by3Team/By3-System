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
/// 对外 API Token 响应。
/// </summary>
public class ExternalApiTokenDto
{
    public Guid Id { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// ApiSecret 仅在创建/重生成时返回，列表查询返回空字符串。
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpireTime { get; set; }
    public string ExpireType { get; set; } = "30";
    public List<Guid> AllowedApiIds { get; set; } = new();
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? PreviousValidUntil { get; set; }
    public string? PreviousApiKey { get; set; }
    public string? PreviousApiSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建对外 API Token 请求。
/// </summary>
public class CreateExternalApiTokenDto
{
    public string AppName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpireTime { get; set; }
    public string ExpireType { get; set; } = "30";
    public List<Guid> AllowedApiIds { get; set; } = new();
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新对外 API Token 请求。
/// </summary>
public class UpdateExternalApiTokenDto
{
    public string AppName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpireTime { get; set; }
    public string ExpireType { get; set; } = "30";
    public List<Guid> AllowedApiIds { get; set; } = new();
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 创建对外 API 访问日志（内部使用）。
/// </summary>
public class CreateExternalApiAccessLogDto
{
    public string ApiKey { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
    public string? RequestParams { get; set; }
    public string? IpAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? IdempotencyKey { get; set; }
}

/// <summary>
/// 对外 API 访问日志响应。
/// </summary>
public class ExternalApiAccessLogDto
{
    public Guid Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
    public string? RequestParams { get; set; }
    public string? IpAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 重新生成 Token 请求参数。
/// </summary>
public class RegenerateExternalApiTokenDto
{
    /// <summary>
    /// 旧 Key 失效方式：0 立即失效；1 指定时间后失效。
    /// </summary>
    public int OldKeyExpireType { get; set; }

    /// <summary>
    /// 旧 Key 缓冲期截止时间（UTC）。OldKeyExpireType=1 时必填。
    /// </summary>
    public DateTime? OldKeyExpireAt { get; set; }
}

/// <summary>
/// Token 操作日志 DTO。
/// </summary>
public class ExternalApiTokenLogDto
{
    public Guid Id { get; set; }
    public Guid TokenId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public Guid? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Token 历史记录响应。
/// </summary>
public class ExternalApiTokenHistoryDto
{
    public Guid Id { get; set; }
    public Guid TokenId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public DateTime? ExpireTime { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? InvalidatedAt { get; set; }
    public Guid? InvalidatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否仍有效（未手动作废且缓冲期未过）。
    /// </summary>
    public bool IsValid => !InvalidatedAt.HasValue && (!ValidUntil.HasValue || ValidUntil.Value >= DateTime.UtcNow);
}
