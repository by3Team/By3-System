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

public class DictDataRepository
{
    private readonly AppDbContext _db;
    public DictDataRepository(AppDbContext db) => _db = db;

    private IQueryable<SysDictData> Queryable() => _db.DictData;

    public async Task<SysDictData?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<List<SysDictData>> GetByTypeIdAsync(Guid dictTypeId)
        => await Queryable()
            .Where(d => d.DictTypeId == dictTypeId)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

    public async Task<List<SysDictData>> GetByTypeCodeAsync(string dictTypeCode)
    {
        var type = await _db.DictTypes.FirstOrDefaultAsync(t => t.DictType == dictTypeCode && !t.IsDeleted);
        if (type == null) return new List<SysDictData>();
        return await GetByTypeIdAsync(type.Id);
    }

    public async Task<List<SysDictData>> GetListAsync(int page, int pageSize, Guid? dictTypeId = null)
    {
        var query = Queryable();
        if (dictTypeId.HasValue)
            query = query.Where(d => d.DictTypeId == dictTypeId.Value);
        return await query
            .OrderBy(d => d.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(Guid? dictTypeId = null)
    {
        var query = Queryable();
        if (dictTypeId.HasValue)
            query = query.Where(d => d.DictTypeId == dictTypeId.Value);
        return await query.CountAsync();
    }

    public async Task<Guid> CreateAsync(SysDictData data)
    {
        _db.DictData.Add(data);
        await _db.SaveChangesAsync();
        return data.Id;
    }

    public async Task<int> UpdateAsync(SysDictData data)
    {
        _db.DictData.Update(data);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var data = await _db.DictData.FindAsync(id);
        if (data == null) return 0;
        data.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }
}
