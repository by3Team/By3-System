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

public class RoleRepository
{
    private readonly AppDbContext _db;
    public RoleRepository(AppDbContext db) => _db = db;

    private IQueryable<SysRole> Queryable() => _db.Roles;

    public async Task<SysRole?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<SysRole>> GetListAsync()
        => await Queryable().OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<List<SysRole>> GetListAsync(int page, int pageSize, string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(r => r.RoleName.Contains(keyword));
        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(r => r.RoleName.Contains(keyword));
        return await query.CountAsync();
    }

    public async Task<Guid> CreateAsync(SysRole role)
    {
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role.Id;
    }

    public async Task<int> UpdateAsync(SysRole role)
    {
        _db.Roles.Update(role);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return 0;
        role.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    public async Task SetRoleMenusAsync(Guid roleId, List<Guid> menuIds)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var existing = _db.RoleMenus.Where(rm => rm.RoleId == roleId);
            _db.RoleMenus.RemoveRange(existing);

            if (menuIds.Count > 0)
            {
                var list = menuIds.Select(mid => new SysRoleMenu
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    MenuId = mid,
                    CreatedAt = DateTime.UtcNow
                }).ToList();
                _db.RoleMenus.AddRange(list);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Guid>> GetMenuIdsByRoleIdAsync(Guid roleId)
        => await _db.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

    /// <summary>
    /// 检查是否有用户关联了指定角色。
    /// </summary>
    public async Task<bool> HasUsersAsync(Guid roleId)
        => await _db.UserRoles.AnyAsync(ur => ur.RoleId == roleId);
}
