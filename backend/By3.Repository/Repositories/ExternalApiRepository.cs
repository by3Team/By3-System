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

public class ExternalApiRepository
{
    private readonly AppDbContext _db;
    public ExternalApiRepository(AppDbContext db) => _db = db;

    private IQueryable<SysExternalApi> Queryable() => _db.ExternalApis;

    public async Task<SysExternalApi?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(t => t.Id == id);

    public async Task<SysExternalApi?> GetByRouteAsync(string route, string method)
        => await Queryable()
            .FirstOrDefaultAsync(t => t.Route == route && t.Method == method.ToUpperInvariant());

    public async Task<List<SysExternalApi>> GetListAsync(int page, int pageSize, string? keyword = null, bool? isEnabled = null)
    {
        var query = BuildSearchQuery(keyword, isEnabled);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword = null, bool? isEnabled = null)
    {
        var query = BuildSearchQuery(keyword, isEnabled);
        return await query.CountAsync();
    }

    public async Task<List<SysExternalApi>> GetAllAsync()
    {
        return await Queryable()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    private IQueryable<SysExternalApi> BuildSearchQuery(string? keyword, bool? isEnabled = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLowerInvariant();
            query = query.Where(t => t.ApiName.ToLower().Contains(lowerKeyword) || t.Route.ToLower().Contains(lowerKeyword));
        }
        if (isEnabled.HasValue)
            query = query.Where(t => t.IsEnabled == isEnabled.Value);
        return query;
    }

    public async Task<Guid> CreateAsync(SysExternalApi api)
    {
        _db.ExternalApis.Add(api);
        await _db.SaveChangesAsync();
        return api.Id;
    }

    public async Task<int> UpdateAsync(SysExternalApi api)
    {
        _db.ExternalApis.Update(api);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var api = await _db.ExternalApis.FindAsync(id);
        if (api == null) return 0;
        api.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string route, string method, Guid? excludeId = null)
    {
        var query = Queryable().Where(t => t.Route == route && t.Method == method.ToUpperInvariant());
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
