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

using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class JobService
{
    private readonly JobRepository _jobRepo;
    private readonly JobLogRepository _jobLogRepo;

    public JobService(JobRepository jobRepo, JobLogRepository jobLogRepo)
    {
        _jobRepo = jobRepo;
        _jobLogRepo = jobLogRepo;
    }

    /// <summary>
    /// 分页查询定时任务列表
    /// </summary>
    public async Task<PageResult<JobDto>> GetListAsync(int page, int pageSize, string? keyword = null, string? isEnabled = null)
    {
        bool? enabled = isEnabled?.ToLowerInvariant() switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => null
        };

        var items = await _jobRepo.GetListAsync(page, pageSize, keyword, enabled);
        var total = await _jobRepo.GetCountAsync(keyword, enabled);

        return new PageResult<JobDto>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items.Select(MapToDto).ToList()
        };
    }

    /// <summary>
    /// 根据ID获取定时任务
    /// </summary>
    public async Task<JobDto?> GetByIdAsync(Guid id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        return job == null ? null : MapToDto(job);
    }

    /// <summary>
    /// 创建定时任务
    /// </summary>
    public async Task<Guid> CreateAsync(JobCreateDto dto)
    {
        var job = new SysJob
        {
            Id = Guid.NewGuid(),
            JobName = dto.JobName,
            JobGroup = dto.JobGroup,
            JobType = dto.JobType,
            CronExpression = dto.CronExpression,
            Description = dto.Description,
            ConfigJson = dto.ConfigJson,
            IsEnabled = dto.IsEnabled,
            CreatedAt = DateTime.UtcNow
        };

        return await _jobRepo.CreateAsync(job);
    }

    /// <summary>
    /// 更新定时任务
    /// </summary>
    public async Task<int> UpdateAsync(Guid id, JobUpdateDto dto)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job == null) return 0;

        job.JobName = dto.JobName;
        job.JobGroup = dto.JobGroup;
        job.JobType = dto.JobType;
        job.CronExpression = dto.CronExpression;
        job.Description = dto.Description;
        job.ConfigJson = dto.ConfigJson;
        job.IsEnabled = dto.IsEnabled;
        job.UpdatedAt = DateTime.UtcNow;

        return await _jobRepo.UpdateAsync(job);
    }

    /// <summary>
    /// 删除定时任务
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
    {
        return await _jobRepo.DeleteAsync(id);
    }

    /// <summary>
    /// 分页查询任务执行日志
    /// </summary>
    public async Task<PageResult<JobLogDto>> GetLogsAsync(JobLogQueryDto query)
    {
        var repoQuery = new JobLogQuery
        {
            JobId = query.JobId,
            JobName = query.JobName,
            Status = query.Status,
            StartTime = query.StartTime,
            EndTime = query.EndTime
        };

        var items = await _jobLogRepo.GetListAsync(query.Page, query.PageSize, repoQuery);
        var total = await _jobLogRepo.GetCountAsync(repoQuery);

        return new PageResult<JobLogDto>
        {
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            Items = items.Select(MapToLogDto).ToList()
        };
    }

    private static JobDto MapToDto(SysJob job) => new()
    {
        Id = job.Id,
        JobName = job.JobName,
        JobGroup = job.JobGroup,
        JobType = job.JobType,
        CronExpression = job.CronExpression,
        Description = job.Description,
        ConfigJson = job.ConfigJson,
        IsEnabled = job.IsEnabled,
        NextFireTime = job.IsEnabled ? QuartzSchedulerHostedService.GetNextFireTimeFromCron(job.CronExpression)?.DateTime : null,
        CreatedAt = job.CreatedAt,
        UpdatedAt = job.UpdatedAt
    };

    private static JobLogDto MapToLogDto(SysJobLog log) => new()
    {
        Id = log.Id,
        JobId = log.JobId,
        JobName = log.JobName,
        FireTime = log.FireTime,
        EndTime = log.EndTime,
        Status = log.Status,
        Result = log.Result,
        ExceptionMessage = log.ExceptionMessage,
        NextFireTime = log.NextFireTime,
        CreatedAt = log.CreatedAt
    };
}
