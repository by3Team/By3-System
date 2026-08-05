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

public class ExternalApiTokenHistoryRepository
{
    private readonly AppDbContext _db;
    public ExternalApiTokenHistoryRepository(AppDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(SysExternalApiTokenHistory history)
    {
        _db.ExternalApiTokenHistories.Add(history);
        await _db.SaveChangesAsync();
        return history.Id;
    }

    public async Task<SysExternalApiTokenHistory?> GetByIdAsync(Guid id)
        => await _db.ExternalApiTokenHistories.FindAsync(id);

    public async Task<List<SysExternalApiTokenHistory>> GetListAsync(Guid tokenId, int page, int pageSize, string? status = null)
    {
        var query = BuildQuery(tokenId, status);
        return await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(Guid tokenId, string? status = null)
    {
        var query = BuildQuery(tokenId, status);
        return await query.CountAsync();
    }

    public async Task<int> InvalidateAsync(Guid id, Guid? userId)
    {
        var history = await _db.ExternalApiTokenHistories.FindAsync(id);
        if (history == null) return 0;
        if (history.InvalidatedAt.HasValue) return 0;

        history.InvalidatedAt = DateTime.UtcNow;
        history.InvalidatedBy = userId;
        return await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 将指定 Token 下所有未失效的历史凭证全部标记为失效。
    /// 用于重新生成 Key/Secret 时保证同一应用最多只有“当前 Key + 上一个缓冲 Key”两个有效凭证。
    /// </summary>
    public async Task<int> InvalidateAllActiveAsync(Guid tokenId, Guid? userId)
    {
        var now = DateTime.UtcNow;
        return await _db.ExternalApiTokenHistories
            .Where(h => h.TokenId == tokenId && !h.InvalidatedAt.HasValue)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(h => h.InvalidatedAt, now)
                .SetProperty(h => h.InvalidatedBy, userId));
    }

    private IQueryable<SysExternalApiTokenHistory> BuildQuery(Guid tokenId, string? status)
    {
        var query = _db.ExternalApiTokenHistories.Where(h => h.TokenId == tokenId);
        var now = DateTime.UtcNow;

        switch (status?.ToLowerInvariant())
        {
            case "valid":
                query = query.Where(h => !h.InvalidatedAt.HasValue && (!h.ValidUntil.HasValue || h.ValidUntil.Value >= now));
                break;
            case "invalid":
                query = query.Where(h => h.InvalidatedAt.HasValue || (h.ValidUntil.HasValue && h.ValidUntil.Value < now));
                break;
        }

        return query;
    }
}
