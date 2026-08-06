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
using By3.Service.DTOs;

namespace By3.Service.Services;

public class JobSchedulerService
{
    private readonly QuartzSchedulerHostedService _hostedService;

    public JobSchedulerService(QuartzSchedulerHostedService hostedService)
    {
        _hostedService = hostedService;
    }

    /// <summary>
    /// 注册或更新定时任务调度。
    /// </summary>
    public Task ScheduleAsync(JobScheduleDto job, CancellationToken cancellationToken = default)
        => _hostedService.ScheduleJobAsync(MapToEntity(job), cancellationToken);

    /// <summary>
    /// 移除指定定时任务调度。
    /// </summary>
    public Task UnscheduleAsync(Guid jobId, string jobGroup, CancellationToken cancellationToken = default)
        => _hostedService.UnscheduleJobAsync(jobId, jobGroup, cancellationToken);

    /// <summary>
    /// 立即触发指定定时任务执行。
    /// </summary>
    public Task TriggerAsync(Guid jobId, string jobGroup, CancellationToken cancellationToken = default)
        => _hostedService.TriggerJobAsync(jobId, jobGroup, cancellationToken);

    /// <summary>
    /// 切换定时任务启用/停用状态。
    /// </summary>
    public async Task<bool> ToggleAsync(JobScheduleDto job, CancellationToken cancellationToken = default)
    {
        if (job.IsEnabled)
        {
            await _hostedService.ScheduleJobAsync(MapToEntity(job), cancellationToken);
        }
        else
        {
            await _hostedService.UnscheduleJobAsync(job.Id, job.JobGroup, cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// 获取指定任务的下次触发时间。
    /// </summary>
    public Task<DateTimeOffset?> GetNextFireTimeAsync(Guid jobId, string jobGroup, CancellationToken cancellationToken = default)
        => _hostedService.GetNextFireTimeAsync(jobId, jobGroup, cancellationToken);

    /// <summary>
    /// 根据 Cron 表达式计算下次触发时间。
    /// </summary>
    public DateTimeOffset? GetNextFireTimeFromCron(string cronExpression)
        => QuartzSchedulerHostedService.GetNextFireTimeFromCron(cronExpression);

    private static SysJob MapToEntity(JobScheduleDto dto) => new()
    {
        Id = dto.Id,
        JobName = dto.JobName,
        JobGroup = dto.JobGroup,
        JobType = dto.JobType,
        CronExpression = dto.CronExpression,
        ConfigJson = dto.ConfigJson,
        IsEnabled = dto.IsEnabled
    };
}
