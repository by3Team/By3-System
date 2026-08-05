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

using Quartz;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.Constants;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Service.Jobs;

/// <summary>
/// 人员数据模拟定时任务（Quartz Job）。
/// </summary>
/// <remarks>
/// 任务职责：
/// 1. 按配置的 BatchSize 批量生成模拟用户数据；
/// 2. 在事务中插入数据库，插入前将现有用户导出为 CSV 备份；
/// 3. 清理超过保留份数的历史备份文件。
///
/// [DisallowConcurrentExecution] 表示同一任务（相同 JobKey）不会并发执行，
/// 避免多个实例同时写库/写文件导致数据混乱。
/// </remarks>
[DisallowConcurrentExecution]
public class UserDataSeedQuartzJob : IJob
{
    private readonly UserSeedJobService _userSeedJobService;
    private readonly JobLogRepository _jobLogRepo;

    public UserDataSeedQuartzJob(UserSeedJobService userSeedJobService, JobLogRepository jobLogRepo)
    {
        _userSeedJobService = userSeedJobService;
        _jobLogRepo = jobLogRepo;
    }

    /// <summary>
    /// Quartz 调度器触发本 Job 时执行的入口。
    /// </summary>
    /// <param name="context">Quartz 执行上下文，包含 JobId、JobName、ConfigJson 等运行时参数。</param>
    public async Task Execute(IJobExecutionContext context)
    {
        // 从 JobDataMap 中读取前端/数据库中配置的任务参数
        var jobId = context.MergedJobDataMap.GetGuidValue("JobId");
        var jobName = context.MergedJobDataMap.GetString("JobName") ?? JobTypes.UserDataSeed;
        var configJson = context.MergedJobDataMap.GetString("ConfigJson") ?? string.Empty;
        var fireTime = DateTime.UtcNow;

        try
        {
            // 调用业务服务执行具体的数据插入、备份、清理逻辑
            var result = await _userSeedJobService.ExecuteAsync(jobId, jobName, configJson, context.CancellationToken);

            // 记录成功日志
            await LogAsync(new SysJobLog
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                JobName = jobName,
                FireTime = fireTime,
                EndTime = DateTime.UtcNow,
                Status = "Success",
                Result = $"插入 {result.InsertedCount} 条用户，备份 {result.BackupFilePath}，清理旧备份 {result.CleanedUpCount} 份",
                NextFireTime = context.NextFireTimeUtc?.DateTime,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // 记录失败日志，并抛出 JobExecutionException 让 Quartz 感知任务失败
            await LogAsync(new SysJobLog
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                JobName = jobName,
                FireTime = fireTime,
                EndTime = DateTime.UtcNow,
                Status = "Failed",
                Result = "执行失败",
                ExceptionMessage = ex.Message,
                NextFireTime = context.NextFireTimeUtc?.DateTime,
                CreatedAt = DateTime.UtcNow
            });
            throw new JobExecutionException($"Job {jobName} failed", ex, false);
        }
    }

    /// <summary>
    /// 写入任务执行日志。
    /// </summary>
    private async Task LogAsync(SysJobLog log)
    {
        await _jobLogRepo.CreateAsync(log);
    }
}
