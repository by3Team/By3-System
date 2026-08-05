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

using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class AuditLogService
{
    private readonly AuditLogRepository _repo;
    public AuditLogService(AuditLogRepository repo) => _repo = repo;

    public async Task<PageResult<AuditLogListDto>> GetListAsync(int page, int pageSize, AuditLogQueryDto? query = null)
    {
        var repoQuery = query == null ? null : new AuditLogQuery
        {
            UserName = query.UserName,
            Keyword = query.Keyword,
            RequestMethod = query.RequestMethod,
            StatusCode = query.StatusCode,
            StartTime = query.StartTime,
            EndTime = query.EndTime
        };
        var items = await _repo.GetListAsync(page, pageSize, repoQuery);
        var total = await _repo.GetCountAsync(repoQuery);
        return new PageResult<AuditLogListDto>
        {
            Total = total,
            Items = items.Select(MapToListDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AuditLogDetailDto?> GetByIdAsync(Guid id)
    {
        var log = await _repo.GetByIdAsync(id);
        return log == null ? null : MapToDetailDto(log);
    }

    public async Task CreateAsync(CreateAuditLogDto dto)
    {
        var log = new SysAuditLog
        {
            UserId = dto.UserId,
            UserName = dto.UserName,
            Action = dto.Action,
            Controller = dto.Controller,
            RequestPath = dto.RequestPath,
            RequestMethod = dto.RequestMethod,
            RequestParams = dto.RequestParams,
            RequestBody = dto.RequestBody,
            RequestHeaders = dto.RequestHeaders,
            ResponseResult = dto.ResponseResult,
            ResponseHeaders = dto.ResponseHeaders,
            StatusCode = dto.StatusCode,
            ExceptionMessage = dto.ExceptionMessage,
            ElapsedMs = dto.ElapsedMs,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent
        };
        await _repo.CreateAsync(log);
    }

    private static AuditLogListDto MapToListDto(SysAuditLog log) => new()
    {
        Id = log.Id,
        UserName = log.UserName,
        Action = log.Action,
        RequestPath = log.RequestPath,
        RequestMethod = log.RequestMethod,
        StatusCode = log.StatusCode,
        ElapsedMs = log.ElapsedMs,
        IpAddress = log.IpAddress,
        CreatedAt = log.CreatedAt,
        HasDetail = !string.IsNullOrWhiteSpace(log.RequestHeaders)
            || !string.IsNullOrWhiteSpace(log.RequestBody)
            || !string.IsNullOrWhiteSpace(log.ResponseResult)
    };

    private static AuditLogDetailDto MapToDetailDto(SysAuditLog log) => new()
    {
        Id = log.Id,
        UserName = log.UserName,
        Action = log.Action,
        Controller = log.Controller,
        RequestPath = log.RequestPath,
        RequestMethod = log.RequestMethod,
        RequestParams = SafeJsonPretty(log.RequestParams),
        RequestBody = SafeJsonPretty(log.RequestBody),
        RequestHeaders = SafeJsonPretty(log.RequestHeaders),
        ResponseResult = SafeJsonPretty(log.ResponseResult),
        ResponseHeaders = SafeJsonPretty(log.ResponseHeaders),
        StatusCode = log.StatusCode,
        ExceptionMessage = log.ExceptionMessage,
        ElapsedMs = log.ElapsedMs,
        IpAddress = log.IpAddress,
        UserAgent = log.UserAgent,
        CreatedAt = log.CreatedAt
    };

    private static string? SafeJsonPretty(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return System.Text.Json.JsonSerializer.Serialize(doc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
