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
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 配置JSON
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 下次执行时间
    /// </summary>
    public DateTime? NextFireTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 定时任务调度信息。
/// </summary>
public class JobScheduleDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 配置JSON
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 创建定时任务请求。
/// </summary>
public class JobCreateDto
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    public string JobGroup { get; set; } = "DEFAULT";

    /// <summary>
    /// 任务类型
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 配置JSON
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新定时任务请求。
/// </summary>
public class JobUpdateDto
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 配置JSON
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 任务执行日志响应。
/// </summary>
public class JobLogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 触发时间
    /// </summary>
    public DateTime FireTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 执行结果
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 下次执行时间
    /// </summary>
    public DateTime? NextFireTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 任务日志查询条件。
/// </summary>
public class JobLogQueryDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid? JobId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 用户种子任务配置。
/// </summary>
public class UserSeedJobConfig
{
    /// <summary>
    /// 批量大小
    /// </summary>
    public int BatchSize { get; set; } = 5;

    /// <summary>
    /// 备份目录
    /// </summary>
    public string BackupDirectory { get; set; } = "./backups/users";

    /// <summary>
    /// 保留备份数
    /// </summary>
    public int KeepBackupCount { get; set; } = 7;
}

/// <summary>
/// 任务执行结果。
/// </summary>
public class JobExecutionResult
{
    /// <summary>
    /// 插入数量
    /// </summary>
    public int InsertedCount { get; set; }

    /// <summary>
    /// 备份文件路径
    /// </summary>
    public string BackupFilePath { get; set; } = string.Empty;

    /// <summary>
    /// 清理数量
    /// </summary>
    public int CleanedUpCount { get; set; }
}
