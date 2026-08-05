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

public class EmailSettingRepository
{
    private readonly AppDbContext _db;

    public EmailSettingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SysEmailSetting?> GetAsync()
    {
        return await _db.EmailSettings.FirstOrDefaultAsync(s => s.IsEnabled);
    }

    public async Task<SysEmailSetting> GetOrCreateDefaultAsync()
    {
        var setting = await GetAsync();
        if (setting != null) return setting;

        setting = new SysEmailSetting
        {
            Id = Guid.NewGuid(),
            SmtpHost = string.Empty,
            SmtpPort = 587,
            Username = string.Empty,
            Password = string.Empty,
            FromName = string.Empty,
            FromAddress = string.Empty,
            EnableSsl = true,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.EmailSettings.Add(setting);
        await _db.SaveChangesAsync();
        return setting;
    }

    public async Task<SysEmailSetting> SaveAsync(SysEmailSetting setting)
    {
        var existing = await _db.EmailSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            setting.Id = Guid.NewGuid();
            setting.CreatedAt = DateTime.UtcNow;
            _db.EmailSettings.Add(setting);
        }
        else
        {
            existing.SmtpHost = setting.SmtpHost;
            existing.SmtpPort = setting.SmtpPort;
            existing.Username = setting.Username;
            existing.Password = setting.Password;
            existing.FromName = setting.FromName;
            existing.FromAddress = setting.FromAddress;
            existing.EnableSsl = setting.EnableSsl;
            existing.IsEnabled = setting.IsEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return existing ?? setting;
    }
}
