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

using Microsoft.Extensions.Hosting;

namespace By3.Service.Services;

/// <summary>
/// 在应用启动阶段执行数据库初始化（建库、建表、种子数据）。
/// 注册为 IHostedService，确保在 QuartzSchedulerHostedService 等依赖数据库的服务之前运行。
/// </summary>
public class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await DatabaseInitializer.InitializeAsync(_serviceProvider);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
