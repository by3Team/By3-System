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

public class AuditLogQuery
{
    public string? UserName { get; set; }
    public string? Keyword { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class AuditLogRepository
{
    private readonly AppDbContext _db;
    public AuditLogRepository(AppDbContext db) => _db = db;

    public async Task CreateAsync(SysAuditLog log)
    {
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<SysAuditLog>> GetListAsync(int page, int pageSize, AuditLogQuery? query = null)
    {
        var q = BuildQuery(query);
        return await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(AuditLogQuery? query = null)
        => await BuildQuery(query).CountAsync();

    private IQueryable<SysAuditLog> BuildQuery(AuditLogQuery? query)
    {
        var q = _db.AuditLogs.AsQueryable();
        if (query == null) return q;

        if (!string.IsNullOrWhiteSpace(query.UserName))
            q = q.Where(a => a.UserName.Contains(query.UserName));

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(a => (a.Action != null && a.Action.Contains(query.Keyword))
                || (a.RequestPath != null && a.RequestPath.Contains(query.Keyword)));

        if (!string.IsNullOrWhiteSpace(query.RequestMethod))
            q = q.Where(a => a.RequestMethod == query.RequestMethod);

        if (query.StatusCode.HasValue)
            q = q.Where(a => a.StatusCode == query.StatusCode.Value);

        if (query.StartTime.HasValue)
            q = q.Where(a => a.CreatedAt >= query.StartTime.Value);

        if (query.EndTime.HasValue)
            q = q.Where(a => a.CreatedAt <= query.EndTime.Value);

        return q;
    }

    public async Task<SysAuditLog?> GetByIdAsync(Guid id)
        => await _db.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
}
