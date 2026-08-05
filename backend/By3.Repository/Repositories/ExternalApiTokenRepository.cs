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

public class ExternalApiTokenRepository
{
    private readonly AppDbContext _db;
    public ExternalApiTokenRepository(AppDbContext db) => _db = db;

    private IQueryable<SysExternalApiToken> Queryable(bool includeDeleted = false)
    {
        var query = _db.ExternalApiTokens.AsQueryable();
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        else
        {
            query = query.Where(t => !t.IsDeleted);
        }
        return query;
    }

    public async Task<SysExternalApiToken?> GetByIdAsync(Guid id, bool includeDeleted = false)
        => await Queryable(includeDeleted).FirstOrDefaultAsync(t => t.Id == id);

    /// <summary>
    /// 根据 ApiKey 查询有效的 Token。会同时匹配当前 Key 以及在缓冲期内的旧 Key。
    /// </summary>
    public async Task<SysExternalApiToken?> GetByApiKeyAsync(string apiKey)
    {
        var now = DateTime.UtcNow;
        return await Queryable()
            .FirstOrDefaultAsync(t =>
                t.ApiKey == apiKey ||
                (t.PreviousApiKey == apiKey && t.PreviousValidUntil.HasValue && t.PreviousValidUntil.Value >= now));
    }

    public async Task<List<SysExternalApiToken>> GetListAsync(int page, int pageSize, string? keyword = null, bool includeDeleted = false, bool? isEnabled = null)
    {
        var query = BuildQuery(keyword, includeDeleted, isEnabled);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword = null, bool includeDeleted = false, bool? isEnabled = null)
    {
        var query = BuildQuery(keyword, includeDeleted, isEnabled);
        return await query.CountAsync();
    }

    public async Task<List<SysExternalApiToken>> GetAllAsync(string? keyword = null, bool includeDeleted = false, bool? isEnabled = null)
    {
        var query = BuildQuery(keyword, includeDeleted, isEnabled);
        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    private IQueryable<SysExternalApiToken> BuildQuery(string? keyword, bool includeDeleted, bool? isEnabled)
    {
        var query = Queryable(includeDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(t => t.AppName.Contains(keyword) || t.ApiKey.Contains(keyword));
        if (isEnabled.HasValue)
            query = query.Where(t => t.IsEnabled == isEnabled.Value);
        return query;
    }

    public async Task<Guid> CreateAsync(SysExternalApiToken token)
    {
        _db.ExternalApiTokens.Add(token);
        await _db.SaveChangesAsync();
        return token.Id;
    }

    public async Task<int> UpdateAsync(SysExternalApiToken token)
    {
        _db.ExternalApiTokens.Update(token);
        return await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 逻辑删除：将 IsDeleted 标记为 true，不会从数据库物理删除。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
    {
        var token = await _db.ExternalApiTokens.FindAsync(id);
        if (token == null) return 0;
        token.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    public async Task<bool> ApiKeyExistsAsync(string apiKey)
        => await Queryable().AnyAsync(t => t.ApiKey == apiKey);

    /// <summary>
    /// 获取授权了指定接口 ID 的未删除 Token 数量。
    /// </summary>
    public async Task<int> GetAuthorizedTokenCountAsync(Guid apiId)
    {
        // AllowedApiIds 为 JSON 数组，为空表示允许全部接口
        var tokens = await Queryable()
            .Where(t => string.IsNullOrEmpty(t.AllowedApiIds) || t.AllowedApiIds.Contains(apiId.ToString()))
            .ToListAsync();

        return tokens.Count(t =>
        {
            if (string.IsNullOrWhiteSpace(t.AllowedApiIds)) return true;
            try
            {
                var ids = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(t.AllowedApiIds);
                return ids == null || ids.Count == 0 || ids.Contains(apiId);
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// 获取所有未删除的 Token（用于统计授权关系）。
    /// </summary>
    public async Task<List<SysExternalApiToken>> GetAllNonDeletedAsync()
        => await Queryable().OrderByDescending(t => t.CreatedAt).ToListAsync();
}
