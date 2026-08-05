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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.Constants;
using By3.Service.Jobs;

namespace By3.Service.Services;

public class QuartzSchedulerHostedService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IJobFactory _jobFactory;
    private readonly IServiceProvider _serviceProvider;
    private IScheduler? _scheduler;

    public QuartzSchedulerHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IServiceProvider serviceProvider)
    {
        _schedulerFactory = schedulerFactory;
        _jobFactory = jobFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        _scheduler.JobFactory = _jobFactory;
        await _scheduler.Start(cancellationToken);

        using var scope = _serviceProvider.CreateScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<JobRepository>();
        var jobs = await jobRepo.GetEnabledAsync();

        foreach (var job in jobs)
        {
            await ScheduleJobAsync(job, cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_scheduler != null)
        {
            await _scheduler.Shutdown(true, cancellationToken);
        }
    }

    public async Task ScheduleJobAsync(SysJob job, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) throw new InvalidOperationException("Scheduler not started");

        var jobType = ResolveJobType(job.JobType);
        if (jobType == null) return;

        var jobKey = new JobKey(job.Id.ToString(), job.JobGroup);
        var triggerKey = new TriggerKey($"{job.Id}_trigger", job.JobGroup);

        // 删除已存在的任务和触发器
        if (await _scheduler.CheckExists(jobKey, cancellationToken))
        {
            await _scheduler.DeleteJob(jobKey, cancellationToken);
        }

        var jobDetail = JobBuilder.Create(jobType)
            .WithIdentity(jobKey)
            .UsingJobData("JobId", job.Id.ToString())
            .UsingJobData("JobName", job.JobName)
            .UsingJobData("ConfigJson", job.ConfigJson)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .WithCronSchedule(job.CronExpression)
            .Build();

        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
    }

    public async Task UnscheduleJobAsync(Guid jobId, string jobGroup, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) return;

        var jobKey = new JobKey(jobId.ToString(), jobGroup);
        if (await _scheduler.CheckExists(jobKey, cancellationToken))
        {
            await _scheduler.DeleteJob(jobKey, cancellationToken);
        }
    }

    public async Task TriggerJobAsync(Guid jobId, string jobGroup, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) throw new InvalidOperationException("Scheduler not started");

        var jobKey = new JobKey(jobId.ToString(), jobGroup);
        if (!await _scheduler.CheckExists(jobKey, cancellationToken))
        {
            // 如果未在调度器中，临时创建一个一次性触发器执行
            using var scope = _serviceProvider.CreateScope();
            var jobRepo = scope.ServiceProvider.GetRequiredService<JobRepository>();
            var job = await jobRepo.GetByIdAsync(jobId);
            if (job == null) throw new InvalidOperationException($"Job {jobId} not found");

            await ScheduleJobAsync(job, cancellationToken);
            jobKey = new JobKey(job.Id.ToString(), job.JobGroup);
        }

        await _scheduler.TriggerJob(jobKey, cancellationToken);
    }

    public async Task<DateTimeOffset?> GetNextFireTimeAsync(Guid jobId, string jobGroup, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) return null;

        var triggerKey = new TriggerKey($"{jobId}_trigger", jobGroup);
        var trigger = await _scheduler.GetTrigger(triggerKey, cancellationToken);
        return trigger?.GetNextFireTimeUtc();
    }

    public static DateTimeOffset? GetNextFireTimeFromCron(string cronExpression)
    {
        try
        {
            var cron = new CronExpression(cronExpression);
            return cron.GetNextValidTimeAfter(DateTimeOffset.UtcNow);
        }
        catch
        {
            return null;
        }
    }

    private static Type? ResolveJobType(string jobType)
    {
        return jobType switch
        {
            JobTypes.UserDataSeed => typeof(UserDataSeedQuartzJob),
            _ => null
        };
    }
}
