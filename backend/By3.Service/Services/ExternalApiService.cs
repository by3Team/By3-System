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
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

/// <summary>
/// 对外 API 接口注册管理服务。
/// </summary>
public class ExternalApiService
{
    private readonly ExternalApiRepository _repository;
    private readonly ExternalApiAccessLogRepository _accessLogRepository;
    private readonly ExternalApiTokenRepository _tokenRepository;

    public ExternalApiService(
        ExternalApiRepository repository,
        ExternalApiAccessLogRepository accessLogRepository,
        ExternalApiTokenRepository tokenRepository)
    {
        _repository = repository;
        _accessLogRepository = accessLogRepository;
        _tokenRepository = tokenRepository;
    }

    /// <summary>
    /// 分页查询对外接口列表。
    /// </summary>
    public async Task<PageResult<ExternalApiDto>> GetListAsync(int page, int pageSize, string? keyword = null, string? isEnabled = null)
    {
        bool? enabled = isEnabled?.ToLowerInvariant() switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => null
        };

        var items = await _repository.GetListAsync(page, pageSize, keyword, enabled);
        var total = await _repository.GetCountAsync(keyword, enabled);

        return new PageResult<ExternalApiDto>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items.Select(MapToDto).ToList()
        };
    }

    /// <summary>
    /// 根据 ID 获取对外接口信息。
    /// </summary>
    public async Task<ExternalApiDto?> GetByIdAsync(Guid id)
    {
        var api = await _repository.GetByIdAsync(id);
        return api == null ? null : MapToDto(api);
    }

    /// <summary>
    /// 根据路由和请求方法获取对外接口信息。
    /// </summary>
    public async Task<ExternalApiDto?> GetByRouteAsync(string route, string method)
    {
        var api = await _repository.GetByRouteAsync(route, method);
        return api == null ? null : MapToDto(api);
    }

    /// <summary>
    /// 创建对外接口。
    /// </summary>
    public async Task<Guid> CreateAsync(CreateExternalApiDto dto)
    {
        if (await _repository.ExistsAsync(dto.Route, dto.Method))
            throw new InvalidOperationException($"接口 {dto.Method} {dto.Route} 已存在");

        var api = new SysExternalApi
        {
            Id = Guid.NewGuid(),
            ApiName = dto.ApiName,
            Route = dto.Route.Trim(),
            Method = dto.Method.Trim().ToUpperInvariant(),
            Description = dto.Description,
            RateLimitPerSecond = dto.RateLimitPerSecond,
            RequireIdempotency = dto.RequireIdempotency,
            IsEnabled = dto.IsEnabled,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(api);
        return api.Id;
    }

    /// <summary>
    /// 更新对外接口信息。
    /// </summary>
    public async Task<int> UpdateAsync(Guid id, UpdateExternalApiDto dto)
    {
        var api = await _repository.GetByIdAsync(id);
        if (api == null) return 0;

        if (await _repository.ExistsAsync(dto.Route, dto.Method, id))
            throw new InvalidOperationException($"接口 {dto.Method} {dto.Route} 已存在");

        api.ApiName = dto.ApiName;
        api.Route = dto.Route.Trim();
        api.Method = dto.Method.Trim().ToUpperInvariant();
        api.Description = dto.Description;
        api.RateLimitPerSecond = dto.RateLimitPerSecond;
        api.RequireIdempotency = dto.RequireIdempotency;
        api.IsEnabled = dto.IsEnabled;
        api.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(api);
    }

    /// <summary>
    /// 删除对外接口。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// 获取授权了指定接口的 Token 数量。
    /// </summary>
    public async Task<int> GetAuthorizedTokenCountAsync(Guid id)
    {
        return await _tokenRepository.GetAuthorizedTokenCountAsync(id);
    }

    /// <summary>
    /// 切换接口启用状态，返回受影响 Token 数量。
    /// </summary>
    public async Task<(bool NewStatus, int AffectedTokenCount)> ToggleStatusAsync(Guid id)
    {
        var api = await _repository.GetByIdAsync(id);
        if (api == null) throw new InvalidOperationException("接口不存在");

        var newStatus = !api.IsEnabled;
        var affectedCount = await _tokenRepository.GetAuthorizedTokenCountAsync(id);

        api.IsEnabled = newStatus;
        api.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(api);

        return (newStatus, affectedCount);
    }

    /// <summary>
    /// 获取接口统计信息：近 30 天 QPS/请求量曲线、成功失败数、最近调用时间、已授权 Token 列表。
    /// </summary>
    public async Task<ExternalApiStatsDto?> GetStatsAsync(Guid id)
    {
        var api = await _repository.GetByIdAsync(id);
        if (api == null) return null;

        var requestPath = $"/api{api.Route}";
        var since = DateTime.UtcNow.Date.AddDays(-29);
        var logs = await _accessLogRepository.GetRecentByPathAsync(requestPath, since);

        var dailyStats = BuildDailyStats(since, logs);
        var allowedTokens = await GetAllowedTokensAsync(api.Id);

        return new ExternalApiStatsDto
        {
            Id = api.Id,
            ApiName = api.ApiName,
            Route = api.Route,
            Method = api.Method,
            RateLimitPerSecond = api.RateLimitPerSecond,
            RequireIdempotency = api.RequireIdempotency,
            IsEnabled = api.IsEnabled,
            TotalRequests = logs.Count,
            SuccessCount = logs.Count(l => l.Status == "Success"),
            FailureCount = logs.Count(l => l.Status != "Success"),
            LastCallAt = logs.Count > 0 ? logs.Max(l => l.CreatedAt) : null,
            DailyStats = dailyStats,
            AllowedTokens = allowedTokens
        };
    }

    /// <summary>
    /// 构建近 30 天每日请求统计。
    /// </summary>
    private static List<ExternalApiDailyStatDto> BuildDailyStats(DateTime since, List<SysExternalApiAccessLog> logs)
    {
        var result = new List<ExternalApiDailyStatDto>();
        var grouped = logs
            .GroupBy(l => l.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => new
            {
                Count = g.Count(),
                Success = g.Count(x => x.Status == "Success"),
                Failure = g.Count(x => x.Status != "Success")
            });

        for (var i = 0; i < 30; i++)
        {
            var date = since.AddDays(i).Date;
            var hasData = grouped.TryGetValue(date, out var stat);
            result.Add(new ExternalApiDailyStatDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = hasData ? stat!.Count : 0,
                SuccessCount = hasData ? stat!.Success : 0,
                FailureCount = hasData ? stat!.Failure : 0
            });
        }

        return result;
    }

    /// <summary>
    /// 获取已授权指定接口的 Token 列表。
    /// </summary>
    private async Task<List<ExternalApiAllowedTokenDto>> GetAllowedTokensAsync(Guid apiId)
    {
        var tokens = await _tokenRepository.GetAllNonDeletedAsync();
        return tokens
            .Where(t => string.IsNullOrWhiteSpace(t.AllowedApiIds) ||
                        ParseAllowedApiIds(t.AllowedApiIds).Contains(apiId))
            .Select(t => new ExternalApiAllowedTokenDto
            {
                Id = t.Id,
                AppName = t.AppName,
                ApiKey = t.ApiKey,
                IsEnabled = t.IsEnabled
            })
            .ToList();
    }

    /// <summary>
    /// 反序列化允许访问的接口 ID 列表。
    /// </summary>
    private static List<Guid> ParseAllowedApiIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<Guid>();
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    /// <summary>
    /// 将对外接口实体映射为 DTO。
    /// </summary>
    private static ExternalApiDto MapToDto(SysExternalApi api) => new()
    {
        Id = api.Id,
        ApiName = api.ApiName,
        Route = api.Route,
        Method = api.Method,
        Description = api.Description,
        RateLimitPerSecond = api.RateLimitPerSecond,
        RequireIdempotency = api.RequireIdempotency,
        IsEnabled = api.IsEnabled,
        IsDeleted = api.IsDeleted,
        CreatedAt = api.CreatedAt,
        UpdatedAt = api.UpdatedAt
    };
}
