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

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using By3.Api.Filters;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Controllers;


/// <summary>
/// 定时任务管理：提供定时任务分页查询、详情、增删改、启用禁用及触发功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly JobService _jobService;
    private readonly JobSchedulerService _schedulerService;

    public JobsController(JobService jobService, JobSchedulerService schedulerService)
    {
        _jobService = jobService;
        _schedulerService = schedulerService;
    }


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="isEnabled">状态：enabled/disabled，默认全部</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "job:list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null, string? isEnabled = null)
    {
        var result = await _jobService.GetListAsync(page, pageSize, keyword, isEnabled);
        return Ok(ApiResult<PageResult<JobDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "job:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound(ApiResult<object>.Error("任务不存在", 404));
        return Ok(ApiResult<JobDto>.Ok(job));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "job:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(JobCreateDto dto)
    {
        var id = await _jobService.CreateAsync(dto);

        if (dto.IsEnabled)
        {
            var job = await _jobService.GetByIdAsync(id);
            if (job != null)
            {
                await _schedulerService.ScheduleAsync(new JobScheduleDto
                {
                    Id = job.Id,
                    JobName = job.JobName,
                    JobGroup = job.JobGroup,
                    JobType = job.JobType,
                    CronExpression = job.CronExpression,
                    ConfigJson = job.ConfigJson,
                    IsEnabled = job.IsEnabled
                });
            }
        }

        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "job:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, JobUpdateDto dto)
    {
        var result = await _jobService.UpdateAsync(id, dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("任务不存在", 404));

        var job = await _jobService.GetByIdAsync(id);
        if (job != null)
        {
            await _schedulerService.UnscheduleAsync(job.Id, job.JobGroup);
            if (job.IsEnabled)
            {
                await _schedulerService.ScheduleAsync(new JobScheduleDto
                {
                    Id = job.Id,
                    JobName = job.JobName,
                    JobGroup = job.JobGroup,
                    JobType = job.JobType,
                    CronExpression = job.CronExpression,
                    ConfigJson = job.ConfigJson,
                    IsEnabled = job.IsEnabled
                });
            }
        }

        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "job:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job != null)
        {
            await _schedulerService.UnscheduleAsync(job.Id, job.JobGroup);
        }

        var result = await _jobService.DeleteAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("任务不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }


    /// <summary>
    /// 立即触发任务。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("{id}/trigger")]
    [Authorize(Policy = "job:trigger")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Trigger(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound(ApiResult<object>.Error("任务不存在", 404));

        await _schedulerService.TriggerAsync(job.Id, job.JobGroup);
        return Ok(ApiResult<object>.Ok(null, "任务已触发"));
    }


    /// <summary>
    /// Toggle。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("{id}/toggle")]
    [Authorize(Policy = "job:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound(ApiResult<object>.Error("任务不存在", 404));

        var isEnabled = !job.IsEnabled;

        var updateDto = new JobUpdateDto
        {
            JobName = job.JobName,
            JobGroup = job.JobGroup,
            JobType = job.JobType,
            CronExpression = job.CronExpression,
            Description = job.Description,
            ConfigJson = job.ConfigJson,
            IsEnabled = isEnabled
        };

        await _jobService.UpdateAsync(id, updateDto);
        await _schedulerService.ToggleAsync(new JobScheduleDto
        {
            Id = job.Id,
            JobName = job.JobName,
            JobGroup = job.JobGroup,
            JobType = job.JobType,
            CronExpression = job.CronExpression,
            ConfigJson = job.ConfigJson,
            IsEnabled = isEnabled
        });

        return Ok(ApiResult<object>.Ok(null, isEnabled ? "任务已启用" : "任务已停用"));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/logs")]
    [Authorize(Policy = "job:list")]
    public async Task<IActionResult> GetLogs(Guid id, int page = 1, int pageSize = 10)
    {
        var result = await _jobService.GetLogsAsync(new JobLogQueryDto
        {
            JobId = id,
            Page = page,
            PageSize = pageSize
        });
        return Ok(ApiResult<PageResult<JobLogDto>>.Ok(result));
    }
}
