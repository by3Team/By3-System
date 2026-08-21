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

public class EmailTemplateRepository
{
    private readonly AppDbContext _db;

    public EmailTemplateRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SysEmailTemplate>> GetListAsync(int page, int pageSize, string? keyword)
    {
        var query = _db.EmailTemplates.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.TemplateName.Contains(keyword) || e.TemplateCode.Contains(keyword));
        return await query.OrderByDescending(e => e.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword)
    {
        var query = _db.EmailTemplates.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.TemplateName.Contains(keyword) || e.TemplateCode.Contains(keyword));
        return await query.CountAsync();
    }

    public async Task<SysEmailTemplate?> GetByIdAsync(Guid id)
        => await _db.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<SysEmailTemplate?> GetByCodeAsync(string code)
        => await _db.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(e => e.TemplateCode == code);

    public async Task<Guid> CreateAsync(SysEmailTemplate template)
    {
        _db.EmailTemplates.Add(template);
        await _db.SaveChangesAsync();
        return template.Id;
    }

    public async Task<int> UpdateAsync(SysEmailTemplate template)
    {
        _db.EmailTemplates.Update(template);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id, Guid? deletedBy = null)
    {
        var template = await _db.EmailTemplates.FindAsync(id);
        if (template == null) return 0;

        var deletedAt = DateTime.UtcNow;

        // 备份关联版本
        var versions = await _db.EmailTemplateVersions
            .Where(v => v.TemplateId == id)
            .ToListAsync();
        foreach (var version in versions)
        {
            _db.EmailTemplateVersionBackups.Add(new SysEmailTemplateVersionBackup
            {
                Id = Guid.NewGuid(),
                OriginalId = version.Id,
                TemplateId = version.TemplateId,
                Version = version.Version,
                Subject = version.Subject,
                Body = version.Body,
                BodyFormat = version.BodyFormat,
                IsEnabled = version.IsEnabled,
                CreatedAt = version.CreatedAt,
                UpdatedAt = version.UpdatedAt,
                CreatedBy = version.CreatedBy,
                UpdatedBy = version.UpdatedBy,
                DeletedAt = deletedAt,
                DeletedBy = deletedBy
            });
            _db.EmailTemplateVersions.Remove(version);
        }

        // 备份模板并物理删除
        _db.EmailTemplateBackups.Add(new SysEmailTemplateBackup
        {
            Id = Guid.NewGuid(),
            OriginalId = template.Id,
            TemplateCode = template.TemplateCode,
            TemplateName = template.TemplateName,
            Description = template.Description,
            IsEnabled = template.IsEnabled,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            UpdatedBy = template.UpdatedBy,
            DeletedAt = deletedAt,
            DeletedBy = deletedBy
        });
        _db.EmailTemplates.Remove(template);

        return await _db.SaveChangesAsync();
    }
}
