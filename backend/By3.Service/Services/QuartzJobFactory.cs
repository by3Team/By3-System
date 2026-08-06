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
using Quartz;
using Quartz.Spi;

namespace By3.Service.Services;

public class QuartzJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    public QuartzJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 通过 DI 容器创建 Job 实例。
    /// </summary>
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobType = bundle.JobDetail.JobType;
        var scope = _serviceProvider.CreateScope();
        var job = (IJob)scope.ServiceProvider.GetRequiredService(jobType);

        if (job is IDisposable disposable)
        {
            // 将 scope 与 job 关联，释放 job 时释放 scope
            return new ScopedJobWrapper(disposable, scope);
        }

        return job;
    }

    /// <summary>
    /// 释放 Job 实例。
    /// </summary>
    public void ReturnJob(IJob job)
    {
        (job as IDisposable)?.Dispose();
    }

    private class ScopedJobWrapper : IJob, IDisposable
    {
        private readonly IDisposable _inner;
        private readonly IServiceScope _scope;

        public ScopedJobWrapper(IDisposable inner, IServiceScope scope)
        {
            _inner = inner;
            _scope = scope;
        }

        public Task Execute(IJobExecutionContext context)
        {
            if (_inner is not IJob job)
                throw new InvalidOperationException("Wrapped instance is not a job");

            return job.Execute(context);
        }

        public void Dispose()
        {
            _inner.Dispose();
            _scope.Dispose();
        }
    }
}
