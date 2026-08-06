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
    /// <summary>
    /// Token 唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// API 访问密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// ApiSecret 仅在创建/重生成时返回，列表查询返回空字符串。
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Token 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 过期类型（天数）
    /// </summary>
    public string ExpireType { get; set; } = "30";

    /// <summary>
    /// 允许访问的 API 接口标识列表
    /// </summary>
    public List<Guid> AllowedApiIds { get; set; } = new();

    /// <summary>
    /// 联系邮箱
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 上一密钥有效截止时间
    /// </summary>
    public DateTime? PreviousValidUntil { get; set; }

    /// <summary>
    /// 上一 API 访问密钥
    /// </summary>
    public string? PreviousApiKey { get; set; }

    /// <summary>
    /// 上一 API 密钥
    /// </summary>
    public string? PreviousApiSecret { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建对外 API Token 请求。
/// </summary>
public class CreateExternalApiTokenDto
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Token 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 过期类型（天数）
    /// </summary>
    public string ExpireType { get; set; } = "30";

    /// <summary>
    /// 允许访问的 API 接口标识列表
    /// </summary>
    public List<Guid> AllowedApiIds { get; set; } = new();

    /// <summary>
    /// 联系邮箱
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新对外 API Token 请求。
/// </summary>
public class UpdateExternalApiTokenDto
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Token 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 过期类型（天数）
    /// </summary>
    public string ExpireType { get; set; } = "30";

    /// <summary>
    /// 允许访问的 API 接口标识列表
    /// </summary>
    public List<Guid> AllowedApiIds { get; set; } = new();

    /// <summary>
    /// 联系邮箱
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 创建对外 API 访问日志（内部使用）。
/// </summary>
public class CreateExternalApiAccessLogDto
{
    /// <summary>
    /// API 访问密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 请求路径
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法
    /// </summary>
    public string RequestMethod { get; set; } = string.Empty;

    /// <summary>
    /// 请求参数
    /// </summary>
    public string? RequestParams { get; set; }

    /// <summary>
    /// 客户端 IP 地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 响应状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 幂等键
    /// </summary>
    public string? IdempotencyKey { get; set; }
}

/// <summary>
/// 对外 API 访问日志响应。
/// </summary>
public class ExternalApiAccessLogDto
{
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// API 访问密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 请求路径
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法
    /// </summary>
    public string RequestMethod { get; set; } = string.Empty;

    /// <summary>
    /// 请求参数
    /// </summary>
    public string? RequestParams { get; set; }

    /// <summary>
    /// 客户端 IP 地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 响应状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
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
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联 Token 标识
    /// </summary>
    public Guid TokenId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// API 访问密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 操作者 IP 地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 操作人标识
    /// </summary>
    public Guid? OperatorId { get; set; }

    /// <summary>
    /// 操作人名称
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Token 历史记录响应。
/// </summary>
public class ExternalApiTokenHistoryDto
{
    /// <summary>
    /// 历史记录唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联 Token 标识
    /// </summary>
    public Guid TokenId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// API 访问密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 有效截止时间
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// 作废时间
    /// </summary>
    public DateTime? InvalidatedAt { get; set; }

    /// <summary>
    /// 作废操作人标识
    /// </summary>
    public Guid? InvalidatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否仍有效（未手动作废且缓冲期未过）。
    /// </summary>
    public bool IsValid => !InvalidatedAt.HasValue && (!ValidUntil.HasValue || ValidUntil.Value >= DateTime.UtcNow);
}
