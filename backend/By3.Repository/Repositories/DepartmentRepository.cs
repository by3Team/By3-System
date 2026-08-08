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

public class DepartmentRepository
{
    private readonly AppDbContext _db;
    public DepartmentRepository(AppDbContext db) => _db = db;

    private IQueryable<SysDepartment> Queryable() => _db.Departments;

    public async Task<SysDepartment?> GetByIdAsync(Guid id)
        => await Queryable().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<List<SysDepartment>> GetAllAsync()
        => await Queryable().OrderBy(d => d.SortOrder).ToListAsync();

    public async Task<Guid> CreateAsync(SysDepartment dept)
    {
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        return dept.Id;
    }

    public async Task<int> UpdateAsync(SysDepartment dept)
    {
        _db.Departments.Update(dept);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept == null) return 0;
        dept.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 检查指定部门是否有子部门。
    /// </summary>
    public async Task<bool> HasChildrenAsync(Guid id)
        => await _db.Departments.AnyAsync(d => d.ParentId == id && !d.IsDeleted);
}
