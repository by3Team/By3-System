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

public class EmailLogRepository
{
    private readonly AppDbContext _db;

    public EmailLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SysEmailLog>> GetListAsync(int page, int pageSize, string? keyword, string? status)
    {
        var query = _db.EmailLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.ToAddresses.Contains(keyword) || e.Subject.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);
        return await query.OrderByDescending(e => e.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword, string? status)
    {
        var query = _db.EmailLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.ToAddresses.Contains(keyword) || e.Subject.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);
        return await query.CountAsync();
    }

    public async Task<Guid> CreateAsync(SysEmailLog log)
    {
        _db.EmailLogs.Add(log);
        await _db.SaveChangesAsync();
        return log.Id;
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? errorMessage, DateTime? sentAt)
    {
        var log = await _db.EmailLogs.FindAsync(id);
        if (log == null) return;
        log.Status = status;
        log.ErrorMessage = errorMessage;
        log.SentAt = sentAt;
        await _db.SaveChangesAsync();
    }
}
