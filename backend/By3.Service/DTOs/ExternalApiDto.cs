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
/// 对外 API 接口信息 DTO。
/// </summary>
public class ExternalApiDto
{
    public Guid Id { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RateLimitPerSecond { get; set; }
    public bool RequireIdempotency { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建对外 API 接口请求 DTO。
/// </summary>
public class CreateExternalApiDto
{
    public string ApiName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RateLimitPerSecond { get; set; } = 0;
    public bool RequireIdempotency { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新对外 API 接口请求 DTO。
/// </summary>
public class UpdateExternalApiDto
{
    public string ApiName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RateLimitPerSecond { get; set; }
    public bool RequireIdempotency { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 对外 API 接口统计信息 DTO。
/// </summary>
public class ExternalApiStatsDto
{
    public Guid Id { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int RateLimitPerSecond { get; set; }
    public bool RequireIdempotency { get; set; }
    public bool IsEnabled { get; set; }

    public int TotalRequests { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime? LastCallAt { get; set; }

    /// <summary>
    /// 最近 30 天每日请求量曲线数据。
    /// </summary>
    public List<ExternalApiDailyStatDto> DailyStats { get; set; } = new();

    /// <summary>
    /// 已配置允许访问该接口的 Token 列表。
    /// </summary>
    public List<ExternalApiAllowedTokenDto> AllowedTokens { get; set; } = new();
}

/// <summary>
/// 对外 API 每日统计。
/// </summary>
public class ExternalApiDailyStatDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

/// <summary>
/// 对外 API 已授权 Token 信息。
/// </summary>
public class ExternalApiAllowedTokenDto
{
    public Guid Id { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
