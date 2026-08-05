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

public class LoginLogQuery
{
    public string? UserName { get; set; }
    public bool? IsSuccess { get; set; }
    public string? Keyword { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class LoginLogRepository
{
    private readonly AppDbContext _db;
    public LoginLogRepository(AppDbContext db) => _db = db;

    public async Task CreateAsync(SysLoginLog log)
    {
        _db.LoginLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<SysLoginLog>> GetListAsync(int page, int pageSize, LoginLogQuery? query = null)
    {
        var q = BuildQuery(query);
        return await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(LoginLogQuery? query = null)
        => await BuildQuery(query).CountAsync();

    private IQueryable<SysLoginLog> BuildQuery(LoginLogQuery? query)
    {
        var q = _db.LoginLogs.AsQueryable();
        if (query == null) return q;

        if (!string.IsNullOrWhiteSpace(query.UserName))
            q = q.Where(l => l.UserName.Contains(query.UserName));

        if (query.IsSuccess.HasValue)
            q = q.Where(l => l.IsSuccess == query.IsSuccess.Value);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(l => (l.Message != null && l.Message.Contains(query.Keyword))
                || (l.IpAddress != null && l.IpAddress.Contains(query.Keyword)));

        if (query.StartTime.HasValue)
            q = q.Where(l => l.CreatedAt >= query.StartTime.Value);

        if (query.EndTime.HasValue)
            q = q.Where(l => l.CreatedAt <= query.EndTime.Value);

        return q;
    }
}
