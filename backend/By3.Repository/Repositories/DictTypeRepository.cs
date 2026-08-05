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

public class DictTypeRepository
{
    private readonly AppDbContext _db;
    public DictTypeRepository(AppDbContext db) => _db = db;

    private IQueryable<SysDictType> Queryable() => _db.DictTypes;

    public async Task<SysDictType?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(t => t.Id == id);

    public async Task<SysDictType?> GetByTypeAsync(string dictType)
        => await Queryable().FirstOrDefaultAsync(t => t.DictType == dictType);

    public async Task<List<SysDictType>> GetListAsync(int page, int pageSize, string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(t => t.DictName.Contains(keyword) || t.DictType.Contains(keyword));
        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(t => t.DictName.Contains(keyword) || t.DictType.Contains(keyword));
        return await query.CountAsync();
    }

    public async Task<Guid> CreateAsync(SysDictType type)
    {
        _db.DictTypes.Add(type);
        await _db.SaveChangesAsync();
        return type.Id;
    }

    public async Task<int> UpdateAsync(SysDictType type)
    {
        _db.DictTypes.Update(type);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var type = await _db.DictTypes.FindAsync(id);
        if (type == null) return 0;
        type.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }
}
