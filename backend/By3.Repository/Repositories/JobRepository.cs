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

public class JobRepository
{
    private readonly AppDbContext _db;
    public JobRepository(AppDbContext db) => _db = db;

    public async Task<List<SysJob>> GetListAsync(int page, int pageSize, string? keyword = null, bool? isEnabled = null)
    {
        var q = BuildQuery(keyword, isEnabled);
        return await q
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? keyword = null, bool? isEnabled = null)
        => await BuildQuery(keyword, isEnabled).CountAsync();

    public async Task<SysJob?> GetByIdAsync(Guid id)
        => await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id);

    public async Task<Guid> CreateAsync(SysJob job)
    {
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return job.Id;
    }

    public async Task<int> UpdateAsync(SysJob job)
    {
        _db.Jobs.Update(job);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var job = await _db.Jobs.FindAsync(id);
        if (job == null) return 0;
        job.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    public async Task<List<SysJob>> GetEnabledAsync()
        => await _db.Jobs.Where(j => j.IsEnabled && !j.IsDeleted).ToListAsync();

    private IQueryable<SysJob> BuildQuery(string? keyword, bool? isEnabled = null)
    {
        var q = _db.Jobs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(j => j.JobName.Contains(keyword) || j.Description.Contains(keyword));
        if (isEnabled.HasValue)
            q = q.Where(j => j.IsEnabled == isEnabled.Value);
        return q;
    }
}

public class JobLogRepository
{
    private readonly AppDbContext _db;
    public JobLogRepository(AppDbContext db) => _db = db;

    public async Task<List<SysJobLog>> GetListAsync(int page, int pageSize, JobLogQuery? query = null)
    {
        var q = BuildQuery(query);
        return await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(JobLogQuery? query = null)
        => await BuildQuery(query).CountAsync();

    public async Task<Guid> CreateAsync(SysJobLog log)
    {
        _db.JobLogs.Add(log);
        await _db.SaveChangesAsync();
        return log.Id;
    }

    private IQueryable<SysJobLog> BuildQuery(JobLogQuery? query)
    {
        var q = _db.JobLogs.AsQueryable();
        if (query == null) return q;

        if (query.JobId.HasValue)
            q = q.Where(l => l.JobId == query.JobId.Value);

        if (!string.IsNullOrWhiteSpace(query.JobName))
            q = q.Where(l => l.JobName.Contains(query.JobName));

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(l => l.Status == query.Status);

        if (query.StartTime.HasValue)
            q = q.Where(l => l.CreatedAt >= query.StartTime.Value);

        if (query.EndTime.HasValue)
            q = q.Where(l => l.CreatedAt <= query.EndTime.Value);

        return q;
    }
}

public class JobLogQuery
{
    public Guid? JobId { get; set; }
    public string? JobName { get; set; }
    public string? Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
