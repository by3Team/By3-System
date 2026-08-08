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

public class PositionRepository
{
    private readonly AppDbContext _db;
    public PositionRepository(AppDbContext db) => _db = db;

    private IQueryable<SysPosition> Queryable() => _db.Positions;

    public async Task<SysPosition?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<SysPosition>> GetListAsync(int page, int pageSize, string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.PositionName.Contains(keyword) || (p.PositionCode != null && p.PositionCode.Contains(keyword)));
        return await query
            .OrderBy(p => p.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.PositionName.Contains(keyword) || (p.PositionCode != null && p.PositionCode.Contains(keyword)));
        return await query.CountAsync();
    }

    public async Task<Guid> CreateAsync(SysPosition position)
    {
        _db.Positions.Add(position);
        await _db.SaveChangesAsync();
        return position.Id;
    }

    public async Task<int> UpdateAsync(SysPosition position)
    {
        _db.Positions.Update(position);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var position = await _db.Positions.FindAsync(id);
        if (position == null) return 0;
        position.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 检查是否有用户属于指定岗位。
    /// </summary>
    public async Task<bool> HasUsersAsync(Guid positionId)
        => await _db.Users.AnyAsync(u => u.PositionId == positionId && !u.IsDeleted);
}
