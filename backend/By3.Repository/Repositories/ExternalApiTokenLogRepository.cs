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

public class ExternalApiTokenLogRepository
{
    private readonly AppDbContext _db;
    public ExternalApiTokenLogRepository(AppDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(SysExternalApiTokenLog log)
    {
        _db.ExternalApiTokenLogs.Add(log);
        await _db.SaveChangesAsync();
        return log.Id;
    }

    public async Task<List<SysExternalApiTokenLog>> GetListAsync(Guid tokenId, int page, int pageSize)
    {
        return await _db.ExternalApiTokenLogs
            .Where(l => l.TokenId == tokenId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(Guid tokenId)
    {
        return await _db.ExternalApiTokenLogs
            .Where(l => l.TokenId == tokenId)
            .CountAsync();
    }
}
