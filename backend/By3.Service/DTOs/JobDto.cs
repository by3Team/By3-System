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

namespace By3.Service.DTOs;

/// <summary>
/// 定时任务响应。
/// </summary>
public class JobDto
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string JobGroup { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime? NextFireTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 定时任务调度信息。
/// </summary>
public class JobScheduleDto
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string JobGroup { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 创建定时任务请求。
/// </summary>
public class JobCreateDto
{
    public string JobName { get; set; } = string.Empty;
    public string JobGroup { get; set; } = "DEFAULT";
    public string JobType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新定时任务请求。
/// </summary>
public class JobUpdateDto
{
    public string JobName { get; set; } = string.Empty;
    public string JobGroup { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 任务执行日志响应。
/// </summary>
public class JobLogDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public DateTime FireTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? ExceptionMessage { get; set; }
    public DateTime? NextFireTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 任务日志查询条件。
/// </summary>
public class JobLogQueryDto
{
    public Guid? JobId { get; set; }
    public string? JobName { get; set; }
    public string? Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 用户种子任务配置。
/// </summary>
public class UserSeedJobConfig
{
    public int BatchSize { get; set; } = 5;
    public string BackupDirectory { get; set; } = "./backups/users";
    public int KeepBackupCount { get; set; } = 7;
}

/// <summary>
/// 任务执行结果。
/// </summary>
public class JobExecutionResult
{
    public int InsertedCount { get; set; }
    public string BackupFilePath { get; set; } = string.Empty;
    public int CleanedUpCount { get; set; }
}
