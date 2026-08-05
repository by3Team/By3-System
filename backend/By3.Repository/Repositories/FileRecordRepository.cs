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

public class FileRecordRepository
{
    private readonly AppDbContext _db;

    public FileRecordRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SysFileRecord>> GetListAsync(int page, int pageSize, string? keyword, string? category)
    {
        var query = _db.FileRecords.AsNoTracking().Where(e => !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.OriginalFileName.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.FileCategory == category);
        return await query.OrderByDescending(e => e.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword, string? category)
    {
        var query = _db.FileRecords.Where(e => !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.OriginalFileName.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.FileCategory == category);
        return await query.CountAsync();
    }

    public async Task<SysFileRecord?> GetByIdAsync(Guid id)
        => await _db.FileRecords.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public async Task<Guid> CreateAsync(SysFileRecord record)
    {
        _db.FileRecords.Add(record);
        await _db.SaveChangesAsync();
        return record.Id;
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var record = await _db.FileRecords.FindAsync(id);
        if (record == null) return 0;
        record.IsDeleted = true;
        record.IsEnabled = false;
        return await _db.SaveChangesAsync();
    }

    public async Task<List<SysFileRecord>> GetAllAsync(string? category = null)
    {
        var query = _db.FileRecords.AsNoTracking().Where(e => !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.FileCategory == category);
        return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    }
}
