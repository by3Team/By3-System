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

public class UserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    private IQueryable<SysUser> Queryable() => _db.Users;

    public async Task<SysUser?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<SysUser>> GetAllWithPhoneAsync()
        => await _db.Users.IgnoreQueryFilters()
            .Where(u => u.Phone != null && u.Phone != string.Empty)
            .ToListAsync();

    public async Task<SysUser?> GetByUserNameAsync(string userName)
        => await Queryable().FirstOrDefaultAsync(u => u.UserName == userName);

    public async Task<List<SysUser>> GetListAsync(int page, int pageSize, string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.UserName.Contains(keyword) || (u.RealName != null && u.RealName.Contains(keyword)));
        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SysUser>> GetRecentListAsync(int? count = null)
    {
        IQueryable<SysUser> query = Queryable().OrderByDescending(u => u.CreatedAt);
        if (count.HasValue && count.Value > 0)
            query = query.Take(count.Value);
        return await query.ToListAsync();
    }

    public async Task<int> InsertRangeAsync(List<SysUser> users)
    {
        await _db.Users.AddRangeAsync(users);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> InsertRangeWithTransactionAsync(List<SysUser> users, Func<Task>? preInsertAction = null)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            if (preInsertAction != null)
                await preInsertAction();

            await _db.Users.AddRangeAsync(users);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return users.Count;
        }
        catch
        {
            await transaction.RollbackAsync();
            _db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<int> GetCountAsync(string? keyword = null)
    {
        var query = Queryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.UserName.Contains(keyword) || (u.RealName != null && u.RealName.Contains(keyword)));
        return await query.CountAsync();
    }

    public async Task<Guid> CreateAsync(SysUser user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public async Task<int> UpdateAsync(SysUser user)
    {
        _db.Users.Update(user);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return 0;
        user.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetRoleIdsByUserIdAsync(Guid userId)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

    public async Task SetUserRolesAsync(Guid userId, List<Guid> roleIds)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var existing = _db.UserRoles.Where(ur => ur.UserId == userId);
            _db.UserRoles.RemoveRange(existing);

            if (roleIds.Count > 0)
            {
                var list = roleIds.Select(rid => new SysUserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = rid,
                    CreatedAt = DateTime.UtcNow
                }).ToList();
                _db.UserRoles.AddRange(list);
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
}
