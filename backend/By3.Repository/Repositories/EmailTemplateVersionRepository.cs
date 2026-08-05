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

public class EmailTemplateVersionRepository
{
    private readonly AppDbContext _db;

    public EmailTemplateVersionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SysEmailTemplateVersion>> GetByTemplateIdAsync(Guid templateId)
        => await _db.EmailTemplateVersions
            .AsNoTracking()
            .Where(e => e.TemplateId == templateId && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<SysEmailTemplateVersion?> GetByIdAsync(Guid id)
        => await _db.EmailTemplateVersions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public async Task<SysEmailTemplateVersion?> GetActiveByTemplateIdAsync(Guid templateId)
        => await _db.EmailTemplateVersions
            .AsNoTracking()
            .Where(e => e.TemplateId == templateId && e.IsEnabled && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<Guid> CreateAsync(SysEmailTemplateVersion version)
    {
        _db.EmailTemplateVersions.Add(version);
        await _db.SaveChangesAsync();
        return version.Id;
    }

    public async Task<int> UpdateAsync(SysEmailTemplateVersion version)
    {
        _db.EmailTemplateVersions.Update(version);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var version = await _db.EmailTemplateVersions.FindAsync(id);
        if (version == null) return 0;
        version.IsDeleted = true;
        version.IsEnabled = false;
        return await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid templateId, string version)
        => await _db.EmailTemplateVersions.AnyAsync(e => e.TemplateId == templateId && e.Version == version && !e.IsDeleted);
}
