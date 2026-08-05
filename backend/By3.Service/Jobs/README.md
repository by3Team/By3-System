# By3.Service.Jobs 定时任务说明

本目录存放 Quartz.NET 的 `IJob` 实现类。Quartz 调度器在 `QuartzSchedulerHostedService` 中启动，
并从数据库读取已启用的任务进行调度。

---

## 1. 如何新增一个定时任务

### 步骤 1：定义任务类型常量

在 `By3.Service/Constants/JobTypes.cs` 中新增一个常量：

```csharp
public const string MyNewJob = "MyNewJob";

public static readonly IReadOnlyList<string> All = new[] { UserDataSeed, MyNewJob };
```

### 步骤 2：实现 IJob

在 `By3.Service/Jobs/` 下新增类，例如 `MyNewJob.cs`：

```csharp
using Quartz;

namespace By3.Service.Jobs;

[DisallowConcurrentExecution]
public class MyNewJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var jobId = context.MergedJobDataMap.GetGuidValue("JobId");
        var configJson = context.MergedJobDataMap.GetString("ConfigJson") ?? string.Empty;

        // 业务逻辑 ...
        await Task.CompletedTask;
    }
}
```

> `[DisallowConcurrentExecution]` 建议加上，防止同一任务并发执行。

### 步骤 3：注册任务类型

在 `By3.Service/Services/QuartzSchedulerHostedService.cs` 的 `ResolveJobType` 方法中注册：

```csharp
private static Type? ResolveJobType(string jobType)
{
    return jobType switch
    {
        JobTypes.UserDataSeed => typeof(UserDataSeedQuartzJob),
        JobTypes.MyNewJob => typeof(MyNewJob),
        _ => null
    };
}
```

### 步骤 4：前端选择任务类型

在 `frontend/src/views/system/task/index.vue` 的任务类型下拉框中增加新选项：

```html
<el-option label="我的新任务" value="MyNewJob" />
```

如果新任务需要额外配置项，可在同一页面扩展 `config` 表单字段。

---

## 2. 前端如何配置任务

前端路径：`/system/task`（任务管理）。

新增/编辑任务时填写：

| 字段 | 说明 | 示例 |
|------|------|------|
| 任务名称 | 任意可读名称 | 每10分钟插入人员数据 |
| 任务分组 | Quartz 分组，默认 `DEFAULT` | DEFAULT |
| 任务类型 | 对应 `JobTypes` 常量 | UserDataSeed |
| Cron 表达式 | Quartz Cron，控制执行周期 | `0 0/10 * * * ?` |
| 批量大小 | UserDataSeed 配置：每次插入数量 | 5 |
| 备份目录 | UserDataSeed 配置：CSV 备份目录 | `./backups/users` |
| 启用状态 | 是否立即调度执行 | 启用 |

### 前端请求路径

前端通过 `jobApi` 调用后端接口（定义在 `frontend/src/api/index.ts`）：

```typescript
export const jobApi = {
  getList: (params: any) => api.get('/v1/jobs', { params }),
  getById: (id: string) => api.get(`/v1/jobs/${id}`),
  create: (data: any) => api.post('/v1/jobs', data),
  update: (id: string, data: any) => api.put(`/v1/jobs/${id}`, data),
  delete: (id: string) => api.delete(`/v1/jobs/${id}`),
  trigger: (id: string) => api.post(`/v1/jobs/${id}/trigger`),
  toggle: (id: string) => api.post(`/v1/jobs/${id}/toggle`),
  getLogs: (id: string, params: any) => api.get(`/v1/jobs/${id}/logs`, { params })
}
```

后端对应控制器：`backend/By3.Api/Controllers/JobsController.cs`。

### UserDataSeed 的 ConfigJson 格式

前端提交时会把 `batchSize` 和 `backupDirectory` 序列化为 JSON：

```json
{
  "batchSize": 5,
  "backupDirectory": "./backups/users",
  "keepBackupCount": 7
}
```

`keepBackupCount` 在前端未暴露，默认值在 `UserSeedJobService` 中处理。

---

## 3. 任务是否开启新线程

**不会为每个任务单独创建新的操作系统线程。**

Quartz.NET 内部维护一个线程池（默认 `SimpleThreadPool`），调度器从线程池中取线程来执行 `IJob.Execute`。
因此：

- `Execute` 方法运行在 Quartz 的工作线程中，不是 ASP.NET 请求线程；
- 多个不同的任务可以并发执行，由 Quartz 线程池大小控制；
- `[DisallowConcurrentExecution]` 保证**同一个 JobKey** 不会同时有两个实例在运行，
  但不同 JobKey 的任务之间互不阻塞；
- 任务内部应使用 `async/await`，避免阻塞 Quartz 工作线程。

---

## 4. UserDataSeedQuartzJob 执行流程

`UserDataSeedQuartzJob` 的执行逻辑：

1. 从 Quartz `JobDataMap` 读取 `JobId`、`JobName`、`ConfigJson`。
2. 调用 `UserSeedJobService.ExecuteAsync`：
   - 解析配置（批量大小、备份目录、保留份数）；
   - 查询当前部门和职位列表，用于生成模拟数据；
   - 生成 `BatchSize` 条随机用户数据；
   - 在数据库事务中插入新用户，并在事务提交前将**当前所有用户**导出为 CSV 备份；
   - 清理超过保留份数的历史备份文件。
3. 写入成功或失败的执行日志到 `SysJobLog` 表。

事务与备份的关系：如果插入失败，事务回滚，但 CSV 备份文件已在事务提交前写入磁盘。
当前实现中备份文件作为灾备冗余，即使回滚也不会删除。
