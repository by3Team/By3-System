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
    /// <summary>
    /// 接口唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 接口名称
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// 请求路由
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法（GET / POST 等）
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 接口描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 每秒请求限制
    /// </summary>
    public int RateLimitPerSecond { get; set; }

    /// <summary>
    /// 是否要求幂等
    /// </summary>
    public bool RequireIdempotency { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }

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
/// 创建对外 API 接口请求 DTO。
/// </summary>
public class CreateExternalApiDto
{
    /// <summary>
    /// 接口名称
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// 请求路由
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法（GET / POST 等）
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 接口描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 每秒请求限制
    /// </summary>
    public int RateLimitPerSecond { get; set; } = 0;

    /// <summary>
    /// 是否要求幂等
    /// </summary>
    public bool RequireIdempotency { get; set; } = true;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新对外 API 接口请求 DTO。
/// </summary>
public class UpdateExternalApiDto
{
    /// <summary>
    /// 接口名称
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// 请求路由
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法（GET / POST 等）
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 接口描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 每秒请求限制
    /// </summary>
    public int RateLimitPerSecond { get; set; }

    /// <summary>
    /// 是否要求幂等
    /// </summary>
    public bool RequireIdempotency { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 对外 API 接口统计信息 DTO。
/// </summary>
public class ExternalApiStatsDto
{
    /// <summary>
    /// 接口唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 接口名称
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// 请求路由
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法（GET / POST 等）
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 每秒请求限制
    /// </summary>
    public int RateLimitPerSecond { get; set; }

    /// <summary>
    /// 是否要求幂等
    /// </summary>
    public bool RequireIdempotency { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 成功请求数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 最后调用时间
    /// </summary>
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
    /// <summary>
    /// 统计日期
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 请求总数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 成功请求数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailureCount { get; set; }
}

/// <summary>
/// 对外 API 已授权 Token 信息。
/// </summary>
public class ExternalApiAllowedTokenDto
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
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}
