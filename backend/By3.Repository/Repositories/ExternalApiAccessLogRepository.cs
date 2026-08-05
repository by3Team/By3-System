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

using Microsoft.EntityFrameworkCore;
using By3.Repository.Entities;

namespace By3.Repository.Repositories;

public class ExternalApiAccessLogRepository
{
    private readonly AppDbContext _db;
    public ExternalApiAccessLogRepository(AppDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(SysExternalApiAccessLog log)
    {
        _db.ExternalApiAccessLogs.Add(log);
        await _db.SaveChangesAsync();
        return log.Id;
    }

    public async Task<List<SysExternalApiAccessLog>> GetListAsync(int page, int pageSize, string? apiKey = null)
    {
        var query = _db.ExternalApiAccessLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(apiKey))
            query = query.Where(l => l.ApiKey == apiKey);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? apiKey = null)
    {
        var query = _db.ExternalApiAccessLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(apiKey))
            query = query.Where(l => l.ApiKey == apiKey);

        return await query.CountAsync();
    }

    /// <summary>
    /// 获取指定请求路径在最近一段时间内的访问日志。
    /// </summary>
    public async Task<List<SysExternalApiAccessLog>> GetRecentByPathAsync(string requestPath, DateTime since)
    {
        return await _db.ExternalApiAccessLogs
            .Where(l => l.RequestPath == requestPath && l.CreatedAt >= since)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }
}
